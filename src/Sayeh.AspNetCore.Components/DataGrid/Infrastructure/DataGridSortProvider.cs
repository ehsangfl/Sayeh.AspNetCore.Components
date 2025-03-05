using System.ComponentModel;
namespace Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

internal class DataGridSortProvider
{

    public IQueryable<TItem> ApplySort<TItem, TValue>(ISortableColumn<TItem,TValue> Column, IQueryable<TItem> Source,bool IsFirst)
    {
        if (Column.SortProperty is null)
            return Source;
        if (IsFirst)
        {
            if (Column.SortDirection!.Value == ListSortDirection.Ascending)
                return Source.OrderBy(Column.SortProperty);
            else
                return Source.OrderByDescending(Column.SortProperty);
        }
        else
        {
            if (Column.SortDirection!.Value == ListSortDirection.Ascending)
                return ((IOrderedQueryable<TItem>)Source).ThenBy(Column.SortProperty);
            else
                return ((IOrderedQueryable<TItem>)Source).ThenByDescending(Column.SortProperty);
        }
    }

}
