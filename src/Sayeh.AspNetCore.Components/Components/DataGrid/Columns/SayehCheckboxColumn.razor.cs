using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components;

public partial class SayehCheckboxColumn<TItem> : IEditableColumn<TItem> where TItem : class
{

    private TItem? Item;
    private bool internalValue { get; set; }

    private bool IsNull;
    private FluentCheckbox? checkboxReference;

    public SayehCheckboxColumn()
    {

    }

    [Parameter]
    public bool IsReadonly { get; set; }

    public async override void SetFocuse()
    {
        await Task.Delay(100);
        if (checkboxReference is not null)
            checkboxReference.FocusAsync();
    }

    public object? GetCurrentValue()
    {
        if (IsNull)
            return null;
        return internalValue;
    }

    public void UpdateSource()
    {
        if (Item is null || InternalIsReadonly) return;
        if (PropertyInfo is not null)
        {
            IsNull = false;
            PropertyInfo.SetValue(Item, internalValue);
        }
    }

    public void BeginEdit(TItem item)
    {
        Item = item;
        var val = _compiledProperty?.Invoke(item);
        if (val is null || !val.HasValue)
        {
            IsNull = true;
            internalValue = false;
        }
        else
        {
            IsNull = false;
            internalValue = val.Value;
        }
    }

    public void CancelEdit()
    {
        Item = null;
    }

    public string? GetEditPropertyPath()
    {
        return PropertyInfo?.Name;
    }

}
