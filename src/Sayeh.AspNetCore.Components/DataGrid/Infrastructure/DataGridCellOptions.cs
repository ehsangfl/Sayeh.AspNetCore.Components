using Microsoft.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

public class DataGridCellOptions<TItem> where TItem : class
{

    public DataGridCellOptions(int collIndex, TItem? item, string? tooltip , string? @class,string? style)
    {
        ColumnIndex = collIndex;
        Item = item;
        Class = @class;
        Style = style;
        Tooltip = tooltip;
    }

    public int? ColumnIndex { get; set; }

    public TItem? Item { get; set; }

    public string? Class { get; set; } 

    public string? Style { get; set; } 

   public string Tooltip { get; set; }

}
