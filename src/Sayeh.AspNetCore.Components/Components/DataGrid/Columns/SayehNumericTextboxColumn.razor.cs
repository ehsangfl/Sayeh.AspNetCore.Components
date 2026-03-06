using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehNumericTextboxColumn<TItem,TValue> : IEditableColumn<TItem> 
        where TItem : class
        where TValue : new()
    {

        private TItem? Item;
        private FluentNumberField<TValue>? element;

        [Parameter]
        public TextFieldType TextFieldType { get; set; } = TextFieldType.Text;

        [Parameter] public bool IsReadonly { get; set; }

        public SayehNumericTextboxColumn()
        {
            
        }

        //private string? _internalValue;
        private TValue? internalValue { get; set; }
        //{
        //    get { return _internalValue; }
        //    set
        //    {
        //        if (value != _internalValue)
        //        {
        //            _internalValue = value;
        //            UpdateSource();
        //        }
        //    }
        //}

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
                PropertyInfo.SetValue(Item, internalValue);
            }
        }

        public void BeginEdit(TItem item)
        {
            Item = item;
            internalValue = _compiledProperty!(item);
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
