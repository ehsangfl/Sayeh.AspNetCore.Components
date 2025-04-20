using System;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using Sayeh.Essentials.Core;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehDatePickerColumn<TItem,TValue> :  IEditableColumn<TItem> 
        where TItem : class
        where TValue : struct
    {

        private TItem? Item;
        private FluentDatePicker? element;

        [Parameter] public bool IsReadonly { get; set; }

        private Nullable<DateTime> internalValue { get; set; }

        public object? GetCurrentValue()
        {
            return internalValue;
        }

        public async override void SetFocuse()
        {
            await Task.Delay(100);
            element?.FocusAsync();
        }

        public void UpdateSource()
        {
            if (Item is null) return;
            if (PropertyInfo is not null)
            {
                if (!internalValue.HasValue && typeof(TValue).IsNullableType())
                    PropertyInfo.SetValue(Item, default(TValue));
                else
                    PropertyInfo.SetValue(Item, internalValue);
            }
        }

        public void BeginEdit(TItem item)
        {
            Item = item;
            internalValue = _compiledProperty!(item).ToDateTime();
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
}
