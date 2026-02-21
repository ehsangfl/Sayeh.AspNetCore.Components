using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehTreeViewCheckboxItem<TItem> : IDisposable where TItem : class
    {

        #region Fields

        PropertyInfo _selectorPath;
        internal Func<TItem, bool> _selectProperty;
        Expression<Func<TItem, bool>>? _selectPropertyExpression;
        internal bool IsValid = false;

        //public bool? CheckState { get => _checkState; private set { _checkState = value; InvokeAsync(StateHasChanged); } }
        public bool? CheckState { get; private set; } = false;


        #endregion

        #region Properties

        [Parameter]
        public Expression<Func<TItem, bool>> SelectProperty { get; set; }

        #endregion

        #region Initialize

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                if (!IsValid)
                {
                    GetIsChecked();
                    UpdateParentsState();
                }
            }
            base.OnAfterRender(firstRender);
        }

        protected override void OnParametersSet()
        {
            if (_selectPropertyExpression != SelectProperty && SelectProperty is not null)
            {
                _selectPropertyExpression = SelectProperty;
                _selectProperty = SelectProperty.Compile();
            }
            if (SelectProperty is null && Owner != null)
            {
                _selectPropertyExpression = Owner.SelectProperty;
                _selectProperty = Owner._selectProperty;
            }
            base.OnParametersSet();
        }

        #endregion

        #region Functions

        bool? GetIsChecked()
        {
            SetIndeterminate();
            return CheckState;
        }

        void UpdateParentsState()
        {
            if (_selectProperty is null || Item is null)
                return;
            if (Parent != null && Parent is SayehTreeViewCheckboxItem<TItem> parent)
            {
                parent.IsValid = false;
                parent.SetIndeterminate();
            }
            else
                InvokeAsync(StateHasChanged);
        }

        void SetIndeterminate()
        {
            if (!_children.Any())
                return;
            if (Item is null || _selectProperty is null)
                return;
            if (!_selectProperty.Invoke(Item))
            {
                if (HasBothCheckedUncheckedChild())
                    CheckState = null;
                else if (hasUncheckedChild())
                    CheckState = false;
                else
                    CheckState = true;
                UpdateParentsState();
            }
            else
                CheckState = true;
            IsValid = true;
        }



        private void CheckedChanged(bool value)
        {
            if (_selectorPath is null && _selectPropertyExpression is not null)
            {
                if (_selectPropertyExpression.Body is MemberExpression memberExpression)
                {
                    _selectorPath = (memberExpression.Member as PropertyInfo)!;

                }
            }
            if (_selectorPath != null)
            {
                _selectorPath.SetValue(Item, value);
            }
            UpdateParentsState();
        }

        private void CheckedStateChanged(bool? value)
        {
            if (!value.HasValue)
            { 
                value = false;
                CheckState = false;
            }
            if (_selectorPath is null && _selectPropertyExpression is not null)
            {
                if (_selectPropertyExpression.Body is MemberExpression memberExpression)
                {
                    _selectorPath = (memberExpression.Member as PropertyInfo)!;

                }
            }
            if (_selectorPath != null && value.HasValue)
            {
                _selectorPath.SetValue(Item, value);
            }

            UpdateParentsState();
            UpdateChildrenState(value ?? false);
        }

        public void UpdateChildrenState(bool? value)
        {
            if (_selectProperty is null || Item is null)
                return;
            if (value.HasValue)
            {
                foreach (var child in _children.Values.Cast<SayehTreeViewCheckboxItem<TItem>>())
                {
                    child.CheckedChanged(value.Value);
                    child.UpdateChildrenState(value);
                }
            }
        }

        bool hasUncheckedChild() => _children?.Select(s => s.Value).Cast<SayehTreeViewCheckboxItem<TItem>>().Any(a => a.Item is not null && a?._selectProperty.Invoke(a.Item) == false && a.CheckState != true) ?? false;

        bool HasBothCheckedUncheckedChild()
        {
            var items = _children?.Select(s => s.Value).Cast<SayehTreeViewCheckboxItem<TItem>>();
            if (items is null)
                return false;

            bool hasChecked = false;
            bool hasUnchecked = false;

            foreach (var child in items)
            {
                if (child is null)
                    continue;

                // Otherwise, if child has children, use its CheckState to infer
                else if (child._children.Any())
                {
                    var cs = child.CheckState;
                    if (cs is null || cs == true)
                        hasChecked = true;
                    if (cs == false)
                        hasUnchecked = true;
                }
                // If child has an Item and a select property, use that boolean
                else if (child.Item is not null && child._selectProperty is not null)
                {
                    var selected = child._selectProperty.Invoke(child.Item);
                    if (selected)
                        hasChecked = true;
                    else
                        hasUnchecked = true;
                }
                if (hasChecked && hasUnchecked)
                    return true;
            }

            return false;
        }

        #endregion


    }
}
