using System.Linq.Expressions;

namespace Sayeh.AspNetCore.Components.Test;

public class StaleStateTestRow
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
}

[TestClass]
public class DataGridColumnOptionsStaleStateTest : TestBase
{
    [TestMethod]
    public async Task OpeningSecondColumnPopup_DoesNotLeaveFirstColumnsPopupStaleInTheDom()
    {
        // Arrange: two text-filterable columns, mirroring Central/Person's Code + FirstName columns.
        var items = Enumerable.Range(0, 20)
            .Select(i => new StaleStateTestRow { Id = i, Code = $"C{i}", FirstName = $"Name{i}" })
            .ToList();

        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehPropertyColumn<StaleStateTestRow, string>));
            builder.AddAttribute(seq++, nameof(SayehPropertyColumn<StaleStateTestRow, string>.Property), (Expression<Func<StaleStateTestRow, string>>)(x => x.Code));
            builder.CloseComponent();
            builder.OpenComponent(seq++, typeof(SayehPropertyColumn<StaleStateTestRow, string>));
            builder.AddAttribute(seq++, nameof(SayehPropertyColumn<StaleStateTestRow, string>.Property), (Expression<Func<StaleStateTestRow, string>>)(x => x.FirstName));
            builder.CloseComponent();
        };

        var cut = Render<SayehDataGrid<StaleStateTestRow>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.Virtualize, true)
            .AddChildContent(gridChild)
        );

        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());
        await cut.InvokeAsync(() => Task.Delay(300));
        cut.Render();

        // Act 1: open the FIRST column's (Code) filter popup - the funnel button toggle.
        // Deliberately NOT calling cut.Render() here: bUnit's IRenderedComponent.Render() forces a
        // full top-down re-render of the root component regardless of which internal StateHasChanged
        // calls actually fired, which would mask the exact bug under test (SayehDataGrid overrides
        // IHandleEvent to suppress automatic re-render, so only an explicit StateHasChanged() call
        // inside the grid's own methods can update its header - calling cut.Render() ourselves would
        // paper over a missing one).
        var firstFunnelButton = cut.FindAll("[aria-haspopup='true']")[0];
        await cut.InvokeAsync(() => firstFunnelButton.Click());
        Assert.IsTrue(cut.Markup.Contains("Column width"), "Sanity check: opening a column's popup should render the grid's column-options wrapper.");

        // Act 2: close it (e.g. via the "Remove filter" link, exactly as ColumnOptions.RemoveFilter does).
        var removeFilterLink = cut.FindAll("fluent-anchor").First(a => a.TextContent.Contains("Remove filter", StringComparison.OrdinalIgnoreCase));
        await cut.InvokeAsync(() => removeFilterLink.Click());

        // Act 3: open the SECOND column's (FirstName) filter popup.
        var secondFunnelButton = cut.FindAll("[aria-haspopup='true']")[1];
        await cut.InvokeAsync(() => secondFunnelButton.Click());

        // Assert: the grid's header must be showing exactly ONE column-options wrapper (FirstName's),
        // not a stale leftover from Code's popup. querySelector('.col-options') in the browser's
        // click-outside-to-close JS only ever finds the FIRST matching element in the whole table -
        // if Code's wrapper is still in the DOM, every click inside FirstName's popup gets checked
        // against the WRONG (Code's) element and is misclassified as "outside", closing it instantly.
        var colOptionsCount = cut.FindAll(".col-options").Count(e => e.TextContent.Contains("Column width"));
        Assert.AreEqual(1, colOptionsCount,
            $"Expected exactly 1 grid column-options wrapper (for FirstName) but found {colOptionsCount} - " +
            "Code's popup wrapper was left stale in the DOM after being closed. Markup: " + cut.Markup);
    }
}
