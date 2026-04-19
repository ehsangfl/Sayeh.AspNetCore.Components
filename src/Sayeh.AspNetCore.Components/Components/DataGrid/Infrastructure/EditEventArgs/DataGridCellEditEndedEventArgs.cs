using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using System.ComponentModel;

namespace Sayeh.AspNetCore.Components;

public class DataGridCellEditEndedEventArgs<TItem> : EventArgs where TItem : class
{

    public DataGridCellEditEndedEventArgs(TItem item,string? propertyName,EditActionEnum action) 
    {
        Item = item;
        PropertyName = propertyName; 
        EditAction = action;
    }

    public TItem Item { get; set; }

	public string? PropertyName { get; set; }

    public EditActionEnum EditAction { get;}

}
