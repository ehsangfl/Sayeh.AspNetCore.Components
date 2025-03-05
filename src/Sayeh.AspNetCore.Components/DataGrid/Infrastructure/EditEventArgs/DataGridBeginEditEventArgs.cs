using System.ComponentModel;

namespace Sayeh.AspNetCore.Components;

public class DataGridBeginEditEventArgs<TItem> : CancelEventArgs where TItem : class
{

    public DataGridBeginEditEventArgs(TItem item,string? propertyName)
    {
        Item = item;
        PropertyName = propertyName; 
    }

    public DataGridBeginEditEventArgs(TItem item)
    {
        Item = item;
    }

    public TItem Item { get; set; }

	public string? PropertyName { get; set; }

}
