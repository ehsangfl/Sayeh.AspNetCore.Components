using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehComboboxColumn<TItem, TValue, TOption> : IEditableColumn<TItem> 
        where TItem : class
        where TOption : notnull
    {
        private TItem? Item;
        private FluentCombobox<TOption>? element;
        private TOption? _selectedItem;
        private Expression<Func<TItem, TOption>>? _lastSelectedItem;
        PropertyInfo? selectedItemPropertyInfo;
        Func<TItem, TOption>? _compiledSelectedItem;

        [EditorRequired, Parameter]
        public IEnumerable<TOption> Items { get; set; } = default!;

        [Parameter]
        public Func<TOption, string> DisplayMember { get; set; } = f => f?.ToString()!;

        [Parameter]
        public Func<TOption, TValue> ValueMember { get; set; } = default!;

        [Parameter]
        public Expression<Func<TItem, TOption>> SelectedItem { get; set; } = default!;

        [Parameter] public bool IsReadonly { get; set; }

        private string? selectedValue { get; set; }

        //private FluentTextField? element;

        public SayehComboboxColumn()
        {

        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (_lastSelectedItem != SelectedItem)
            {
                _lastSelectedItem = SelectedItem;
                _compiledSelectedItem = SelectedItem.Compile();
                if (SelectedItem.Body is MemberExpression memberExpression)
                    selectedItemPropertyInfo = memberExpression.Member as PropertyInfo;
            }
        }

        public object? GetCurrentValue()
        {
            //===== these line of code is setted for html select and we can remove it after resolve fluent ui bug
            SetSelectedItem();
            //


            if (_selectedItem is null)
                return null;
            if (ValueMember is null)
                return _selectedItem;
            else
                return ValueMember.Invoke(_selectedItem);
        }

        public async override void SetFocuse()
        {
            await Task.Delay(100);
            element?.Element.FocusAsync();
        }

        public void UpdateSource()
        {  
            //===== these line of code is setted for html select and we can remove it after resolve fluent ui bug
            SetSelectedItem();
            //

            if (Item is null) return;
            if (PropertyInfo is not null && ValueMember is not null)
                PropertyInfo.SetValue(Item, GetCurrentValue());
            if (selectedItemPropertyInfo is not null)
                selectedItemPropertyInfo.SetValue(Item, _selectedItem);
        }

        public void BeginEdit(TItem item)
        {
            Item = item;
            var initialValue = _compiledProperty!.Invoke(Item);
            if (initialValue is null)
                _selectedItem = default(TOption);
            else if (ValueMember is not null && Items is not null)
                _selectedItem = Items.FirstOrDefault(f => EqualityComparer<TValue>.Default.Equals(ValueMember!.Invoke(f), initialValue));
        }

        public void CancelEdit()
        {
            Item = null;
        }

        public string? GetEditPropertyPath()
        {
            return PropertyInfo?.Name;
        }

        private void SetSelectedItem()
        {
            if (string.IsNullOrEmpty(selectedValue))
                _selectedItem = default(TOption);
            else if (ValueMember is not null)
                _selectedItem = Items.FirstOrDefault(f => selectedValue.Equals(ValueMember.Invoke(f)?.ToString()));
        }
    }
}
