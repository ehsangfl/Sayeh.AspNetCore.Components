
using Microsoft.AspNetCore.Components.Rendering;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components;
partial class RowHeaderColumn<TItem> : SayehColumnBase<TItem>, IEditableColumn<TItem> where TItem : class
{
    public RowHeaderColumn()
    {
        Class = "row-header";
        Width = "1.5rem";
        MinWidth = "1.5rem";
    }

    public bool IsReadonly { get => true; set { } }

    public void BeginEdit(TItem Item)
    {
        
    }

    public void CancelEdit()
    {
        
    }

    public object? GetCurrentValue()
    {
        return null;
    }

    public string? GetEditPropertyPath()
    {
        return null;
    }

    public override void SetFocuse()
    {

    }

    public void UpdateSource()
    {
        
    }

}
