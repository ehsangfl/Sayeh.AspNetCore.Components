using System.ComponentModel;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components;

public class DataGridRowEditEndedEventArgs<TItem> : EventArgs where TItem : class
{

    public DataGridRowEditEndedEventArgs(TItem item,EditActionEnum action) 
    {
        Item = item;
        EditAction = action;
    }

    public TItem Item { get; set; }

    public EditActionEnum EditAction { get;}

}
