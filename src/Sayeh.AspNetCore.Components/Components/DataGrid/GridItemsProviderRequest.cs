using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components;

/// <summary>
/// Parameters for data to be supplied by a <see cref="FluentDataGrid{TGridItem}"/>'s <see cref="FluentDataGrid{TGridItem}.ItemsProvider"/>.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the grid.</typeparam>
public readonly struct GridItemsProviderRequest<TItem>  where TItem :class
{
    /// <summary>
    /// The zero-based index of the first item to be supplied.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// If set, the maximum number of items to be supplied. If not set, the maximum number is unlimited.
    /// </summary>
    public int? Count { get; init; }

    /// <summary>
    /// Specifies which columns represents the sort order.
    ///
    /// Rather than inferring the sort rules manually, you should normally call either <see cref="ISortableColumn{TGridItem}.ApplySort(IEnumerable{TGridItem}, bool)"/>
    /// since they also account for <see cref="SortByColumns" /> automatically.
    /// </summary>
    public IEnumerable<SayehColumnBase<TItem>>? SortByColumns { get; }

    /// <summary>
    /// Specifies which columns represents filter.
    /// </summary>
    public IEnumerable<SayehColumnBase<TItem>>? FilterByColumns { get; }

    internal readonly DataGridSortProvider _sortProvider;

    /// <summary>
    /// A token that indicates if the request should be cancelled.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    internal GridItemsProviderRequest(
        int startIndex, int? count
        , IEnumerable<SayehColumnBase<TItem>>? sortByColumn
        , IEnumerable<SayehColumnBase<TItem>>? filterByColumns
        , CancellationToken cancellationToken)
    {
        StartIndex = startIndex;
        Count = count;
        SortByColumns = sortByColumn;
        FilterByColumns = filterByColumns;
        CancellationToken = cancellationToken;
        _sortProvider = new DataGridSortProvider();
    }

    /// <summary>
    /// Applies the request's filters and sorting rules to the supplied <see cref="IQueryable{TGridItem}"/>.
    ///
    /// </summary>
    /// <param name="source">An <see cref="IQueryable{TGridItem}"/>.</param>
    /// <returns>A new <see cref="IQueryable{TGridItem}"/> representing the <paramref name="source"/> with sorting and filter rules applied.</returns>
    public IEnumerable<TItem>? ApplyFilterAndSorting(IEnumerable<TItem>? source)
    {
        if (source is null)
            return null;
        var filteredSource = ApplyFilter(source);
        return ApplySorting(filteredSource);
    }

    /// <summary>
    /// Applies the request's sorting rules to the supplied <see cref="IQueryable{TGridItem}"/>.
    ///
    /// Note that this only works if the current <see cref="SortByColumns"/> implements <see cref="ISortableColumn{TGridItem,TValue}"/>,
    /// otherwise it will throw.
    /// </summary>
    /// <param name="source">An <see cref="IQueryable{TGridItem}"/>.</param>
    /// <returns>A new <see cref="IQueryable{TGridItem}"/> representing the <paramref name="source"/> with sorting rules applied.</returns>
    public IEnumerable<TItem>? ApplySorting(IEnumerable<TItem> source) 
    {
        if (source is null)
            return null;
        if (SortByColumns is not null)
        {
            IEnumerable<TItem>? orderedItems = null;
            foreach (var col in SortByColumns.Cast<ISortableColumn<TItem>>().OrderBy(s => s.SortOrder))
            {
                if (col.SortDirection.HasValue)
                {
                    orderedItems = col.ApplySort(orderedItems is null ? source : orderedItems, orderedItems is null);
                }
            }
            return orderedItems ?? source;
        }
        return null;
    }

    private IEnumerable<TItem> ApplyFilter(IEnumerable<TItem> source)
    {
        if (FilterByColumns is not null)
        {
            foreach (var col in FilterByColumns.Cast<IFilterableColumn<TItem>>())
            {
                source = col.ApplyFilter(source);
            }
            return source;
        }
        return source;
    }
}
