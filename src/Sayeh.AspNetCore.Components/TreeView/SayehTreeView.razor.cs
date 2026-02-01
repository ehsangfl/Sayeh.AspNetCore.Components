using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Utilities;
using NetTopologySuite.Index.HPRtree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehTreeView<TItem> : FluentComponentBase, IDisposable where TItem : class
    {

        #region Fields

        readonly Dictionary<string, SayehTreeViewItem<TItem>> _allItems = [];
        readonly Debounce _currentSelectedChangedDebounce = new();
        SayehTreeViewItem<TItem>? _selectedNode;
        bool _disposed;
        TItem? _selectedItem;
        Expression<Func<TItem, bool>> _selectPropertyExpression;
        internal Func<TItem, bool> _selectProperty;

        #endregion

        #region Properties

        [Parameter]
        public IEnumerable<TItem>? Items { get; set; }

        [Parameter]
        public Func<TItem, IEnumerable<TItem>>? Children { get; set; }

        [Parameter]
        public TItem? SelectedItem { get; set; }

        [Parameter]
        public RenderFragment<TItem>? ItemTemplate { get; set; }

        [Parameter]
        public bool Virtualize { get; set; }

        [Parameter]
        public Func<TItem, string>? Text { get; set; }

        [Parameter]
        public Func<TItem, TItem?>? ParentItem { get; set; }

        [Parameter]
        public Expression<Func<TItem, bool>> SelectProperty { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Called when <see cref="SelectedItem"/> changes.
        /// Only when using the <see cref="Items"/> property.
        /// </summary>
        [Parameter]
        public EventCallback<TItem?> SelectedItemChanged { get; set; }

        /// <summary>
        /// Called whenever <see cref="SayehTreeViewItem.Expanded"/> changes on an
        /// item within the tree.
        /// You cannot update <see cref="SayehTreeViewItem"/> properties.
        /// </summary>
        [Parameter]
        public EventCallback<TItem> OnExpandedChange { get; set; }

        #endregion

        #region Initialization

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TreeChangeEventArgs))]
        public SayehTreeView()
        {

        }

        protected override async Task OnParametersSetAsync()
        {
            if (!EqualityComparer<TItem>.Default.Equals(_selectedItem, SelectedItem))
            {
                if (_allItems is not null && _allItems.Any())
                   await DisplaySelectedItem(SelectedItem);
                else
                    _currentSelectedChangedDebounce.Run(50, () => InvokeAsync(async () => await DisplaySelectedItem(SelectedItem)));
            }
            if (_selectPropertyExpression != SelectProperty)
            {
                _selectPropertyExpression = SelectProperty;
                _selectProperty = _selectPropertyExpression.Compile();
            }
            base.OnParametersSet();
        }

        #endregion

        #region Functions

        internal async Task ItemSelectedChangeAsync(FluentTreeItem item)
        {
            //if (OnSelectedChange.HasDelegate)
            //{
            //    await OnSelectedChange.InvokeAsync(item);
            //}
        }

        internal void HandleCurrentSelectedChange(TreeChangeEventArgs args)
        {
            if (!_allItems.TryGetValue(args.AffectedId!, out SayehTreeViewItem<TItem>? treeItem))
            {
                return;
            }

            var previouslySelected = _selectedNode;
            _currentSelectedChangedDebounce.Run(50, () => InvokeAsync(async () =>
            {
                _selectedNode = treeItem?.Selected == true ? treeItem : null;
                //if (_selectedNode != previouslySelected && CurrentSelectedChanged.HasDelegate)
                //{
                //    foreach (FluentTreeItem item in _allItems.Values)
                //    {
                //        if (item != CurrentSelected && item.Selected)
                //        {
                //            item.SetSelected(false);
                //        }
                //    }
                //    await CurrentSelectedChanged.InvokeAsync(CurrentSelected);
                //}

                if (Items != null)
                {

                    SelectedItem = args.Selected == true ? _selectedNode?.Item : null;
                    _selectedItem = SelectedItem;
                    if (SelectedItemChanged.HasDelegate)
                    {
                        await SelectedItemChanged.InvokeAsync(SelectedItem);
                    }
                }
            }));
        }

        internal void ItemSelectedChange(SayehTreeViewItem<TItem> node)
        {
            if (_selectedNode != node)
                SetSelectedNode(node);
        }

        internal async Task ItemExpandedChangeAsync(SayehTreeViewItem<TItem> item)
        {
            if (OnExpandedChange.HasDelegate)
            {
                await OnExpandedChange.InvokeAsync(item.Item);
            }
        }

        internal void Register(SayehTreeViewItem<TItem> treeItem)
        {
            ArgumentNullException.ThrowIfNull(treeItem);
            _allItems[treeItem.Id!] = treeItem;
            treeItem.Parent?.Register(treeItem);
        }

        internal void Unregister(SayehTreeViewItem<TItem> treeItem)
        {
            ArgumentNullException.ThrowIfNull(treeItem);
            _allItems.Remove(treeItem.Id!);
            treeItem.Parent?.Unregister(treeItem);
        }

        private void SetSelectedNode(SayehTreeViewItem<TItem> node)
        {
            SelectedItem = node.Item;
            if (SelectedItemChanged.HasDelegate)
                SelectedItemChanged.InvokeAsync(SelectedItem);
            if (_selectedNode is not null)
                _selectedNode.SetSelected(false);
            node.SetSelected(true);
            _selectedNode = node;
            if (node.Expanded || (node.Parent?.Expanded ?? true))
                return;
            var parentsNode = findCollapsedParents(node);
            foreach (var item in parentsNode)
            {
                item.SetExpanded(true);
            }
        }

        private Stack<SayehTreeViewItem<TItem>> findCollapsedParents(SayehTreeViewItem<TItem> node)
        {
            var parents = new Stack<SayehTreeViewItem<TItem>>();
            if (node.Parent is null)
                return parents;
            if (!node.Parent.Expanded)
            {
                parents.Push(node.Parent);
                foreach (var pr in findCollapsedParents(node.Parent).Reverse())
                    parents.Push(pr);
            }
            return parents;
        }

        private async ValueTask DisplaySelectedItem(TItem? selectedItem)
        {
            _selectedItem = selectedItem;
            if (selectedItem is null)
                return;
            if (this.ParentItem is not null)
            {
                var parents = findParents(selectedItem);
                if (parents.Count > 0)
                {
                    var firtParent = parents.Pop();
                    var firstNode = _allItems.FirstOrDefault(f => f.Value.Item == firtParent);
                    if (firstNode.Value is not null)
                    {
                        if (!firstNode.Value.Expanded)
                        {
                            firstNode.Value.SetExpanded(true);
                            if (Virtualize)
                                await Task.Delay(100);
                        }
                        SayehTreeViewItem<TItem>? lastParent = firstNode.Value;
                        while (parents.TryPop(out var item))
                        {
                            var parent = lastParent._children.First(f => f.Value.Item == item);
                            lastParent = parent.Value;
                            if (!lastParent.Expanded)
                            {
                                lastParent.SetExpanded(true);
                                if (Virtualize)
                                    await Task.Delay(100);
                            }
                        }
                        if (lastParent is not null)
                        {
                            if (_selectedNode is not null)
                                _selectedNode.SetSelected(false);
                            _selectedNode = lastParent._children.FirstOrDefault(w => w.Value.Item == selectedItem).Value;
                            _selectedNode.SetSelected(true);
                        }

                    }
                }
            }
            else if (!Virtualize)
            {
               var  node = _allItems.FirstOrDefault(f => f.Value.Item == SelectedItem).Value;
                if (node is not null) {
                    SetSelectedNode(node);
                }
            }
        }

        private Stack<TItem> findParents(TItem item)
        {
            ArgumentNullException.ThrowIfNull(ParentItem);
            var parents = new Stack<TItem>();
            var parent = ParentItem(item);
            if (parent is not null)
            {
                parents.Push(parent);
                foreach (var pr in findParents(parent).Reverse())
                    parents.Push(pr);
            }
            return parents;
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _currentSelectedChangedDebounce?.Dispose();
                _allItems.Clear();
            }

            _disposed = true;
        }

        #endregion

    }
}
