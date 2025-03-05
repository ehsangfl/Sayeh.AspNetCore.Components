using Microsoft.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

public class DataGridRowOptions<TItem> where TItem : class
{

    public DataGridRowOptions(int rowIndex, TItem item, string? gridTemplateColumns, IEnumerable<SayehColumnBase<TItem>>? columns)
    {
        RowIndex = rowIndex;
        Item = item;
        GridTemplateColumns = gridTemplateColumns;
        Columns = columns;
    }

    public int? RowIndex { get; set; }

    public TItem? Item { get; set; }

    public string? GridTemplateColumns { get; set; } = null;

    public IEnumerable<SayehColumnBase<TItem>>? Columns { get; set; }

}
