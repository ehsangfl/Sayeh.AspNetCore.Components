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
    partial class SayehHierarchyColumn<TItem, TValue, TOption> : IEditableColumn<TItem>
        where TItem : class
        where TOption : class
    {

        SayehHierarchyPicker<TOption>? element;

        [Parameter]
        public Func<TOption, TOption?>? Parent { get; set; }

        [Parameter]
        public Func<TOption, IEnumerable<TOption>>? Children { get; set; }

        [Parameter]
        public RenderFragment<TOption>? ItemTemplate { get; set; }

        [Parameter]
        public RenderFragment<TOption>? TreeItemTemplate { get; set; }

        [Parameter]
        public EventCallback<OptionsSearchEventArgs<TOption>> Filter { get; set; }

        public async override void SetFocuse()
        {
            await Task.Delay(100);
            element?.Element.FocusAsync();
        }
    }
}
