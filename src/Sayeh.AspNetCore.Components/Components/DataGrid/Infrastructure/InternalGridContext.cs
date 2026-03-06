using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.AspNetCore.Components.Infrastructure;
using System.ComponentModel;

namespace Sayeh.AspNetCore.Components;

// The grid cascades this so that descendant columns can talk back to it. It's an internal type
// so that it doesn't show up by mistake in unrelated components.
internal sealed class InternalGridContext<TItem> where TItem : class
{
    private int _index = 0;
    private int _rowId = 0;
    private int _cellId = 0;

    public Dictionary<string, SayehDataGridRow<TItem>> Rows { get; set; } = [];
    public int TotalItemCount { get; set; }
    public int TotalViewItemCount { get; set; }
    public IEnumerable<TItem> Items { get; set; }

    public SayehDataGrid<TItem> Grid { get; }
    public EventCallbackSubscribable<object?> ColumnsFirstCollected { get; } = new();
    public SayehSelectColumn<TItem>? SelectColumn;

    public InternalGridContext(SayehDataGrid<TItem> grid)
    {
        Grid = grid;
    }

    public int GetNextRowId()
    {
        Interlocked.Increment(ref _rowId);
        return _rowId;
    }

    public int GetNextCellId()
    {
        Interlocked.Increment(ref _cellId);
        return _cellId;
    }

    internal void ResetRowIndexes(int start)
    {
        _index = start;
    }

    internal void Register(SayehDataGridRow<TItem> row)
    {
        Rows.Add(row.RowId, row);
        if (!Grid.Virtualize)
        {
            row.setRowIndex(_index++);
        }
    }

    public void ApplySelectedItems(bool selected) {
        foreach (var row in Rows) {
            row.Value.setSelected(selected);
        }
    }

    internal void Unregister(SayehDataGridRow<TItem> row)
    {
        Rows.Remove(row.RowId);
    }
}
