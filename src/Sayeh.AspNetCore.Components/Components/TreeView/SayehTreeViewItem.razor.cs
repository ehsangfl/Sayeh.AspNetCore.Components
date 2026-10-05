using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        internal INotifyCollectionChanged? _observedChildren;

        #endregion

        #region Properties

        [Parameter]
        public TItem? Item { get; set; }

        [Parameter]
        public RenderFragment<TItem>? Template { get; set; }

        [Parameter]
        public RenderFragment<TItem>? ChildContent { get; set; }

        /// <summary>
        /// Rendered at the end of the item's row (after the Template/DisplayMember content).
        /// Revealed with an animation when the mouse hovers the row.
        /// </summary>
        [Parameter]
        public RenderFragment<TItem>? Actions { get; set; }

        [CascadingParameter]
        public SayehTreeView<TItem>? Owner { get; set; }

        [Parameter]
        public Func<TItem, TItem>? Parent { get; set; }

        [Parameter]
        public Func<TItem, IEnumerable<TItem>>? Children { get; set; }

        [Parameter]
        public Func<TItem, string>? DisplayMember { get; set; }

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

        /// <summary>
        /// Returns <see cref="Children"/> of <see cref="Item"/> and, when the returned collection implements
        /// <see cref="INotifyCollectionChanged"/>, keeps this node subscribed to it so adding/removing a child
        /// re-renders only this node.
        /// </summary>
        internal IEnumerable<TItem> GetChildItems()
        {
            var items = (Item is not null ? Children?.Invoke(Item) : null) ?? [];
            if (!ReferenceEquals(_observedChildren, items))
            {
                if (_observedChildren is not null)
                    _observedChildren.CollectionChanged -= OnChildrenCollectionChanged;
                _observedChildren = items as INotifyCollectionChanged;
                if (_observedChildren is not null)
                    _observedChildren.CollectionChanged += OnChildrenCollectionChanged;
            }
            return items;
        }

        void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => _ = InvokeAsync(async () =>
            {
                if (_disposed)
                    return;
                if (Owner is not null)
                    await Owner.HandleSourceCollectionChangedAsync(sender, e, this);
                OnChildrenChanged(e);
                StateHasChanged();
            });

        /// <summary>
        /// Called after an observed <see cref="Children"/> collection of this node changed, before re-rendering.
        /// </summary>
        protected virtual void OnChildrenChanged(NotifyCollectionChangedEventArgs e) { }

        void setParentExpanded(SayehTreeViewItem<TItem> parent)
        {
            parent?.SetExpanded(true);
            if (parent?.ParentNode is not null)
                setParentExpanded(parent.ParentNode);
        }

        /// <summary>
        /// fluent-tree-item does not dispatch a "selectedchange" custom event on a plain mouse
        /// click (only HandleExpandedChangeAsync's expand/collapse event reliably fires), so
        /// selecting an item by clicking it never reached <see cref="HandleSelectedChange"/>.
        /// This explicit click handler drives selection directly instead of depending on that
        /// event. @onclick:stopPropagation keeps a child item's click from also re-selecting
        /// every ancestor item it bubbles through.
        /// </summary>
        internal async Task HandleRowClick()
        {
            if (Disabled)
            {
                return;
            }

            SetSelected(true);

            if (Owner != null)
            {
                await Owner.ItemSelectedChangeAsync(this);
            }
        }

        internal async Task HandleSelectedChange(TreeChangeEventArgs args)
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
                await Owner.ItemSelectedChangeAsync(this);
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
                if (_observedChildren is not null)
                    _observedChildren.CollectionChanged -= OnChildrenCollectionChanged;
                _observedChildren = null;
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
