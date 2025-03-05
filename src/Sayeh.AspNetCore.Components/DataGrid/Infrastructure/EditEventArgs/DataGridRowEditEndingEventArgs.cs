using System.ComponentModel;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components;

public class DataGridRowEditEndingEventArgs<TItem> : CancelEventArgs where TItem : class
{

    public DataGridRowEditEndingEventArgs(TItem item,EditActionEnum action) 
    {
        Item = item;
        EditAction = action;
    }

    public TItem Item { get; set; }

    public EditActionEnum EditAction { get;}

}
