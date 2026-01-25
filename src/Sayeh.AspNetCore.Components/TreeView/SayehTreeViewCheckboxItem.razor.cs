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
        Func<TItem, bool> _selectProperty;
        Expression<Func<TItem, bool>>? _selectPropertyExpression;

        #endregion

        #region Properties

        [Parameter]
        public Expression<Func<TItem, bool>> SelectProperty { get; set; }

        #endregion

        #region Initialize

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Owner?.Register(this);
            if (InitiallySelected)
            {
                SetSelected(true);
            }
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

        private void ItemSelectedChanged(bool value)
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
        }

        #endregion


    }
}
