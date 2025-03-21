namespace Sayeh.AspNetCore.Components;

/// <summary>
/// Holds data being supplied to a <see cref="SayehDataGrid{TItem}"/>'s <see cref="SayehDataGrid{TItem}.ItemsProvider"/>.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the grid.</typeparam>
public readonly struct GridItemsProviderResult<TItem>
{
    /// <summary>
    /// The items being supplied.
    /// </summary>
    public /*required*/  ICollection<TItem> Items { get; init; }

    /// <summary>
    /// The total number of items that may be displayed in the grid. This normally means the total number of items in the
    /// underlying data source after applying any filtering that is in effect.
    ///
    /// If the grid is paginated, this should include all pages. If the grid is virtualized, this should include the entire scroll range.
    /// </summary>
    public int TotalItemCount { get; init; }

}

/// <summary>
/// Provides convenience methods for constructing <see cref="GridItemsProviderResult{TItem}"/> instances.
/// </summary>
public static class GridItemsProviderResult
{
    // This is just to provide generic type inference, so you don't have to specify TGridItem yet again.

    /// <summary>
    /// Supplies an instance of <see cref="GridItemsProviderResult{TItem}"/>.
    /// </summary>
    /// <typeparam name="TGridItem">The type of data represented by each row in the grid.</typeparam>
    /// <param name="items">The items being supplied.</param>
    /// <param name="totalItemCount">The total numer of items that exist. See <see cref="GridItemsProviderResult{TItem}.TotalItemCount"/> for details.</param>
    /// <returns>An instance of <see cref="GridItemsProviderResult{TItem}"/>.</returns>
    public static GridItemsProviderResult<Ttem> From<Ttem>(ICollection<Ttem> items, int totalItemCount)
        => new() { Items = items, TotalItemCount = totalItemCount };
}
