// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Linq;


namespace Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

/// <summary>
/// provides a sort order specification used within <see cref="SayehDataGrid{TItem}"/>.
/// </summary>
internal class SortProvider
{
    public IEnumerable<TItem> ApplySort<TItem, TValue>(ISortableColumn<TItem, TValue> Column, IEnumerable<TItem> Source, bool IsFirst)
    {
        if (Column.SortProperty is null)
            return Source;
        if (IsFirst)
        {
            if (Column.SortDirection!.Value == ListSortDirection.Ascending)
                return Source.OrderBy(Column.SortProperty.Compile());
            else
                return Source.OrderByDescending(Column.SortProperty.Compile());
        }
        else
        {
            if (Column.SortDirection!.Value == ListSortDirection.Ascending)
                return ((IOrderedEnumerable<TItem>)Source).ThenBy(Column.SortProperty.Compile());
            else
                return ((IOrderedEnumerable<TItem>)Source).ThenByDescending(Column.SortProperty.Compile());
        }
    }
}
