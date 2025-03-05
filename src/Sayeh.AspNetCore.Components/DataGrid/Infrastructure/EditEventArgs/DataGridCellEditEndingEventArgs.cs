using System.ComponentModel;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
namespace Sayeh.AspNetCore.Components;

public class DataGridCellEditEndingEventArgs<TItem> : CancelEventArgs where TItem : class
{

    public DataGridCellEditEndingEventArgs(TItem item,string? propertyName,object? newValue,EditActionEnum action) 
    {
        Item = item;
        PropertyName = propertyName; 
        EditAction = action;
        NewValue = newValue;
    }

    public TItem Item { get; set; }

	public string? PropertyName { get; set; }

    public EditActionEnum EditAction { get;}

    public object? NewValue { get; }

}
