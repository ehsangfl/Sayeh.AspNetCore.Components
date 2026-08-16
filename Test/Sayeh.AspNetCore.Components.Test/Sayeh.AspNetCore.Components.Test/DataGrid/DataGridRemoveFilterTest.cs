using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using System.Linq.Expressions;
using System.Reflection;

namespace Sayeh.AspNetCore.Components.Test;

public class RemoveFilterTestRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

[TestClass]
public class DataGridRemoveFilterTest : TestBase
{
    private static int GetTotalItemCount<TItem>(SayehDataGrid<TItem> grid) where TItem : class
    {
        var contextField = typeof(SayehDataGrid<TItem>).GetField("_internalGridContext", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var gridContext = contextField.GetValue(grid)!;
        return (int)gridContext.GetType().GetProperty("TotalItemCount")!.GetValue(gridContext)!;
    }

    [TestMethod]
    public async Task RemoveFilterButtonClick_ThroughRealEventDispatch_ClearsTheFilter()
    {
        // Arrange: mirrors the reported production scenario - a text (string) filterable column,
        // like Central/Person's Code / FirstName / LastName columns.
        var items = Enumerable.Range(0, 200)
            .Select(i => new RemoveFilterTestRow { Id = i, Name = $"Item{i}" })
            .ToList();

        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehPropertyColumn<RemoveFilterTestRow, string>));
            builder.AddAttribute(seq++, nameof(SayehPropertyColumn<RemoveFilterTestRow, string>.Property), (Expression<Func<RemoveFilterTestRow, string>>)(x => x.Name));
            builder.CloseComponent();
        };

        var cut = Render<SayehDataGrid<RemoveFilterTestRow>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.Virtualize, true)
            .AddChildContent(gridChild)
        );

        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());
        await cut.InvokeAsync(() => Task.Delay(300));
        cut.Render();
        Assert.AreEqual(items.Count, GetTotalItemCount(cut.Instance), "Sanity check: initial unfiltered count.");

        var propertyColumn = cut.FindComponent<SayehPropertyColumn<RemoveFilterTestRow, string>>().Instance;
        var colOptionsInstance = cut.FindComponent<ColumnOptions<RemoveFilterTestRow, string>>().Instance;

        // Act 1: apply a text filter exactly as OnTextFilterSetted does in production (this is the
        // real grid-level ApplyFilter call, already proven correct by the other test) - narrows the
        // grid down to items containing "Item1" (Item1, Item10-19, Item100-199 = 12 matches).
        colOptionsInstance.TextFilter = "Item1";
        await cut.InvokeAsync(() => cut.Instance.ApplyFilter((IFilterableColumn<RemoveFilterTestRow>)propertyColumn));
        await cut.InvokeAsync(() => Task.Delay(300));
        cut.Render();

        var filteredCount = GetTotalItemCount(cut.Instance);
        Assert.IsTrue(filteredCount > 0 && filteredCount < items.Count,
            $"Sanity check failed: expected a partial match, got {filteredCount} of {items.Count}.");

        // Act 2: open the column options popup and click the real "Remove filter" button, exactly
        // as the user does - this exercises ColumnOptions.RemoveFilter()'s actual fire-and-forget
        // call to Owner.Grid.ApplyFilter(), not a direct C# call bypassing that dispatch path.
        // The filter funnel button is ColumnOptions' own toggle (aria-haspopup="true"), distinct
        // from the grid-level column-resize-options popup.
        var funnelButton = cut.Find("[aria-haspopup='true']");
        await cut.InvokeAsync(() => funnelButton.Click());
        cut.Render();

        var removeFilterLink = cut.FindAll("fluent-anchor").FirstOrDefault(a => a.TextContent.Contains("Remove filter", StringComparison.OrdinalIgnoreCase));
        Assert.IsNotNull(removeFilterLink, "Could not find the 'Remove filter' link in the rendered column options popup. Markup: " + cut.Markup);

        await cut.InvokeAsync(() => removeFilterLink!.Click());
        // Give the fire-and-forget ApplyFilter call (started inside RemoveFilter) time to complete.
        await cut.InvokeAsync(() => Task.Delay(300));
        cut.Render();

        // Assert: clicking "Remove filter" must actually clear the filter and restore the full count.
        var afterRemoveCount = GetTotalItemCount(cut.Instance);
        Assert.AreEqual(items.Count, afterRemoveCount,
            $"Clicking 'Remove filter' should restore the full unfiltered count ({items.Count}) but TotalItemCount is still {afterRemoveCount}.");
    }
}
