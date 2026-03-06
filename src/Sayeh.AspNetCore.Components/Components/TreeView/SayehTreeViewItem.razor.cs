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
    partial class SayehTreeViewItem<TItem> : IDisposable where TItem : class
    {

        #region Fields

        protected bool _disposed;
        internal readonly Dictionary<string, SayehTreeViewItem<TItem>> _children = [];
        SayehTreeViewItem<TItem>? _parent;

        #endregion

        #region Properties

        [Parameter]
        public TItem? Item { get; set; }

        [Parameter]
        public RenderFragment<TItem>? ItemTemplate { get; set; }

        [Parameter]
        public RenderFragment<TItem>? ChildrenTemplate { get; set; }

        [CascadingParameter]
        public SayehTreeView<TItem>? Owner { get; set; }

        [Parameter]
        public Func<TItem, TItem>? ParentItem { get; set; }

        [Parameter]
        public Func<TItem, IEnumerable<TItem>>? Children { get; set; }

        [Parameter]
        public Func<TItem, string>? Text { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        //[Parameter]
        public bool Expanded { get; set; }

        [Parameter]
        public bool InitiallySelected { get; set; }

        [CascadingParameter]
        public SayehTreeViewItem<TItem>? ParentNode { get; set; }

        [Parameter]
        public bool Selected { get; set; }


        #endregion

        #region Initialize

        public SayehTreeViewItem()
        {
            this.Id = Identifier.NewId();
        }

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
            base.OnParametersSet();
            if (Selected && ParentNode != _parent)
            {
                _parent = ParentNode;
                if (ParentNode is not null)
                    setParentExpanded(ParentNode);
            }
        }

        #endregion

        #region Functions

        internal void SetSelected(bool value)
        {
            if (value == Selected)
            {
                return;
            }

            Selected = value;
        }

        void setParentExpanded(SayehTreeViewItem<TItem> parent)
        {
            parent?.SetExpanded(true);
            if (parent?.ParentNode is not null)
                setParentExpanded(parent.ParentNode);
        }

        internal void HandleSelectedChange(TreeChangeEventArgs args)
        {
            if (args.AffectedId != Id || args.Selected is null || args.Selected == Selected)
            {
                return;
            }

            if (Owner?.Items == null)
            {
                SetSelected(args.Selected.Value);
            }

            if (Owner != null)
            {
                Selected = args.Selected.Value;
                //await Owner.ItemSelectedChangeAsync(this);
            }
        }

        internal async Task HandleExpandedChangeAsync(TreeChangeEventArgs args)
        {
            if (args.AffectedId != Id || args.Expanded is null || args.Expanded == Expanded)
            {
                return;
            }

            Expanded = args.Expanded.Value;

            if (Owner != null)
            {
                await Owner.ItemExpandedChangeAsync(this);
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        internal void Register(SayehTreeViewItem<TItem> treeItem)
        {
            ArgumentNullException.ThrowIfNull(treeItem);
            ArgumentNullException.ThrowIfNull(treeItem.Id);
            _children[treeItem.Id] = treeItem;
        }

        internal void Unregister(SayehTreeViewItem<TItem> treeItem)
        {
            ArgumentNullException.ThrowIfNull(treeItem);
            ArgumentNullException.ThrowIfNull(treeItem.Id);
            _children.Remove(treeItem.Id);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Owner?.Unregister(this);
            }

            _disposed = true;
        }

        internal void SetExpanded(bool expanded) { Expanded = expanded; InvokeAsync(StateHasChanged); }

        #endregion

        #region API


        public SayehTreeViewItem<TItem>? GetNode(TItem item) => _children.FirstOrDefault(f => f.Value.Item == item).Value;

        #endregion

    }
}
