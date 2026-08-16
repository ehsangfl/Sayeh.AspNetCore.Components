using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using System.Linq.Expressions;
using System.Reflection;

namespace Sayeh.AspNetCore.Components.Test;

[TestClass]
public class DataGridVirtualizeFilterTest : TestBase
{
    private static List<CustomTypeModel> GenerateLargeDataset(int count, DateTime baseDate)
    {
        var items = new List<CustomTypeModel>(count);
        for (var i = 0; i < count; i++)
        {
            items.Add(new CustomTypeModel
            {
                ID = Guid.NewGuid(),
                Order = i,
                // Spread dates across +/-500 days so a narrow filter window matches only a small subset.
                Date = baseDate.AddDays((i % 1000) - 500)
            });
        }
        return items;
    }

    private static int GetTotalItemCount<TItem>(SayehDataGrid<TItem> grid) where TItem : class
    {
        var contextField = typeof(SayehDataGrid<TItem>).GetField("_internalGridContext", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var gridContext = contextField.GetValue(grid)!;
        return (int)gridContext.GetType().GetProperty("TotalItemCount")!.GetValue(gridContext)!;
    }

    [TestMethod]
    public async Task VirtualizedFilter_UpdatesTotalItemCount_ToFilteredCount()
    {
        // Arrange: a "big data" style dataset, spread over a wide date range - mirrors the
        // production scenario (Central/Person page) that triggered this investigation:
        // a large virtualized, unpaginated grid bound directly to an Items collection.
        var baseDate = new DateTime(2026, 1, 1);
        var items = GenerateLargeDataset(5000, baseDate);

        // A 2-day window should match roughly 5000 * 2/1000 = ~10 items - a small subset of the total.
        var fromDate = baseDate.AddDays(-500);
        var toDate = fromDate.AddDays(2);
        var expectedMatches = items.Where(i => i.Date >= fromDate && i.Date <= toDate).ToList();
        Assert.IsTrue(expectedMatches.Count > 0 && expectedMatches.Count < items.Count / 10,
            $"Test setup sanity check failed: expected a small non-zero subset, got {expectedMatches.Count} of {items.Count}");

        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehPropertyColumn<CustomTypeModel, DateTime>));
            builder.AddAttribute(seq++, nameof(SayehPropertyColumn<CustomTypeModel, DateTime>.Property), (Expression<Func<CustomTypeModel, DateTime>>)(x => x.Date));
            builder.CloseComponent();
        };

        var cut = Render<SayehDataGrid<CustomTypeModel>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.Virtualize, true)
            .AddChildContent(gridChild)
        );

        // The automatic "first columns collected -> first load" bootstrap relies on a <Defer>
        // render pass that doesn't reliably fire under bUnit's synchronous harness, so force the
        // initial load explicitly via the same public API RefreshDataAsync() that production uses.
        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());
        await cut.InvokeAsync(() => Task.Delay(300));
        cut.Render();

        Assert.AreEqual(items.Count, GetTotalItemCount(cut.Instance),
            "Sanity check failed: initial unfiltered TotalItemCount should equal the full dataset size.");

        var propertyColumn = cut.FindComponent<SayehPropertyColumn<CustomTypeModel, DateTime>>().Instance;

        // Act: apply the date-range filter exactly as ColumnOptions.DateRangeSelected does in
        // production (set the filter state on the rendered ColumnOptions instance, then trigger
        // the grid's real ApplyFilter -> RefreshDataAsync -> Virtualize.RefreshDataAsync pipeline).
        var colOptionsInstance = cut.FindComponent<ColumnOptions<CustomTypeModel, DateTime>>().Instance;
        colOptionsInstance.FromDate = fromDate;
        colOptionsInstance.ToDate = toDate;
        await cut.InvokeAsync(() => cut.Instance.ApplyFilter((IFilterableColumn<CustomTypeModel>)propertyColumn));
        // The virtualized item provider debounces by 100ms before resolving.
        await cut.InvokeAsync(() => Task.Delay(300));
        cut.Render();

        // Assert: TotalItemCount must reflect the FILTERED count, not the raw unfiltered dataset
        // size. If this is wrong, the grid's aria-rowcount and the <Virtualize> component's own
        // scrollable-range accounting stay sized for the full dataset, so any index past the real
        // (small) filtered result set returns zero items forever - i.e. the grid gets permanently
        // stuck rendering virtualization placeholders for rows that will never resolve.
        var actualTotalItemCount = GetTotalItemCount(cut.Instance);
        Assert.AreEqual(expectedMatches.Count, actualTotalItemCount,
            $"TotalItemCount should reflect the filtered result count ({expectedMatches.Count}) but was {actualTotalItemCount} " +
            $"(the full unfiltered dataset is {items.Count}). A filtered grid stuck reporting the unfiltered total " +
            "causes virtualization to request rows beyond the real result set, which never resolve - permanently stuck placeholders.");

        // Act: clear the filter (mirrors ColumnOptions.RemoveFilter).
        colOptionsInstance.FromDate = null;
        colOptionsInstance.ToDate = null;
        await cut.InvokeAsync(() => cut.Instance.ApplyFilter((IFilterableColumn<CustomTypeModel>)propertyColumn));
        await cut.InvokeAsync(() => Task.Delay(300));
        cut.Render();

        // Assert: clearing the filter must restore the full unfiltered count.
        Assert.AreEqual(items.Count, GetTotalItemCount(cut.Instance),
            "TotalItemCount should return to the full dataset size after the filter is cleared.");
    }
}
