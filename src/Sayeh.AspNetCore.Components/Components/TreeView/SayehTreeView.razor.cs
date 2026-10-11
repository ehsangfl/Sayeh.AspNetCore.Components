using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Utilities;
using Microsoft.JSInterop;
using NetTopologySuite.Index.HPRtree;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehTreeView<TItem> : FluentComponentBase, IDisposable, IAsyncDisposable where TItem : class
    {

        #region Fields

        readonly Dictionary<string, SayehTreeViewItem<TItem>> _allItems = [];
        readonly Debounce _currentSelectedChangedDebounce = new();
        internal  SayehTreeViewItem<TItem>? _selectedNode;
        bool _disposed;
        TItem? _selectedItem;
        Expression<Func<TItem, bool>> _selectPropertyExpression;
        internal Func<TItem, bool> _selectProperty;

        // Keeps one generated key per item instance (does not persist across replacements)
        private readonly ConditionalWeakTable<TItem, string> _generatedKeys = new();

        INotifyCollectionChanged? _observedItems;

        private const string JAVASCRIPT_FILE = "./_content/Sayeh.AspNetCore.Components/Components/TreeView/SayehTreeView.razor.js";
        IJSObjectReference? _module;
        IJSObjectReference? _dragDropHandle;
        DotNetObjectReference<SayehTreeView<TItem>>? _dotNetRef;
        bool? _dragDropCheckDrop;
        bool _dragDropInitializing;
        // The item being moved by an ItemDropped handler: its removal from the source collection must not clear the selection.
        TItem? _movingItem;


        #endregion

        #region Properties

        [Parameter]
        public IEnumerable<TItem>? Items { get; set; }

        /// <summary>
        /// SayehTreeview finds children of each item and render child nodes
        /// </summary>
        [Parameter]
        public Func<TItem, IEnumerable<TItem>>? Children { get; set; }

        /// <summary>
        /// With this property, SayehTreeview finds parent item of the each item to can keep sync the state of parent and childs
        /// </summary>
        [Parameter]
        public Func<TItem, TItem?>? Parent { get; set; }

        [Parameter]
        public TItem? SelectedItem { get; set; }

        [Parameter]
        public RenderFragment<TItem>? ItemTemplate { get; set; }

        /// <summary>
        /// Rendered at the end of each item's row, revealed with an animation on mouse hover.
        /// </summary>
        [Parameter]
        public RenderFragment<TItem>? ItemActions { get; set; }

        [Parameter]
        public bool Virtualize { get; set; }

        /// <summary>
        /// if ItemTemplate does not setted, SayehTreeView uses this property to render a text for each node
        /// </summary>
        [Parameter]
        public Func<TItem, string>? DisplayMember { get; set; }

        [Parameter]
        public Func<TItem, bool>? IsReadOnlyMember { get; set; }

        [Parameter]
        public Expression<Func<TItem, bool>> SelectProperty { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Lets the user reorder/re-parent items by dragging them. The tree does not move the data itself:
        /// handle <see cref="ItemDropped"/> or <see cref="ItemDroppedCommand"/> to update the source collections.
        /// </summary>
        [Parameter]
        public bool AllowDragDrop { get; set; }

        /// <summary>
        /// Decides whether an item can be dragged. When not set, every item can be dragged (if <see cref="AllowDragDrop"/> is enabled).
        /// </summary>
        [Parameter]
        public Func<TItem, bool>? CanDrag { get; set; }

        /// <summary>
        /// Decides whether the dragged item can be dropped at the given place. Dropping an item on itself or on one
        /// of its descendants is always refused. <see cref="ItemDroppedCommand"/>'s CanExecute is checked as well.
        /// </summary>
        [Parameter]
        public Func<TreeViewItemDropEventArgs<TItem>, bool>? CanDrop { get; set; }

        /// <summary>
        /// Milliseconds the dragged item must hover the middle of a collapsed node before that node is expanded.
        /// </summary>
        [Parameter]
        public int DragExpandDelay { get; set; } = 800;

        [Inject]
        IJSRuntime JSRuntime { get; set; } = default!;

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

        [Parameter]
        public EventCallback<TItem> ItemChecked { get; set; }

        [Parameter]
        public EventCallback<TItem> ItemUnchecked { get; set; }

        /// <summary>
        /// Executed with the item as parameter when an item is checked, alongside <see cref="ItemChecked"/>.
        /// </summary>
        [Parameter]
        public ICommand? ItemCheckedCommand { get; set; }

        /// <summary>
        /// Executed with the item as parameter when an item is unchecked, alongside <see cref="ItemUnchecked"/>.
        /// </summary>
        [Parameter]
        public ICommand? ItemUncheckedCommand { get; set; }

        /// <summary>
        /// Called when an item is dropped (see <see cref="AllowDragDrop"/>). The handler moves the item in the source collections.
        /// </summary>
        [Parameter]
        public EventCallback<TreeViewItemDropEventArgs<TItem>> ItemDropped { get; set; }

        /// <summary>
        /// Executed with a <see cref="TreeViewItemDropEventArgs{TItem}"/> as parameter when an item is dropped, alongside <see cref="ItemDropped"/>.
        /// Its CanExecute is evaluated while dragging (when the item enters a node, for each drop position): a position it
        /// refuses shows no drop indicator, and a refused drop raises neither the command nor <see cref="ItemDropped"/>.
        /// </summary>
        [Parameter]
        public ICommand? ItemDroppedCommand { get; set; }

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
                _selectProperty = _selectPropertyExpression?.Compile()!;
            }
            if (!ReferenceEquals(_observedItems, Items))
            {
                if (_observedItems is not null)
                    _observedItems.CollectionChanged -= OnItemsCollectionChanged;
                _observedItems = Items as INotifyCollectionChanged;
                if (_observedItems is not null)
                    _observedItems.CollectionChanged += OnItemsCollectionChanged;
            }
            base.OnParametersSet();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!AllowDragDrop || _disposed || _dragDropInitializing)
                return;
            try
            {
                // The JS module only asks .NET whether a drop is allowed when something can refuse it.
                var checkDrop = CanDrop is not null || ItemDroppedCommand is not null || OverridesDragEnter;
                if (_dragDropHandle is null)
                {
                    // A render while the module import is awaited must not register the listeners twice.
                    _dragDropInitializing = true;
                    _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
                    _dotNetRef ??= DotNetObjectReference.Create(this);
                    _dragDropHandle = await _module.InvokeAsync<IJSObjectReference>("init", Element, _dotNetRef,
                        new { checkDrop, expandDelay = DragExpandDelay });
                    _dragDropCheckDrop = checkDrop;
                    if (_disposed)
                    {
                        await _dragDropHandle.InvokeVoidAsync("stop");
                        await _dragDropHandle.DisposeAsync();
                        _dragDropHandle = null;
                    }
                }
                else if (_dragDropCheckDrop != checkDrop)
                {
                    _dragDropCheckDrop = checkDrop;
                    await _dragDropHandle.InvokeVoidAsync("setOptions", new { checkDrop, expandDelay = DragExpandDelay });
                }
            }
            catch (JSException ex)
            {
                Console.WriteLine("[SayehTreeView] " + ex.Message);
            }
            catch (Exception ex) when (ex is JSDisconnectedException || ex is OperationCanceledException)
            {
                // The JS side may already be gone if the client disconnected.
            }
            finally
            {
                _dragDropInitializing = false;
            }
        }

        #endregion

        #region Functions

        internal async Task ItemSelectedChangeAsync(SayehTreeViewItem<TItem> item)
        {
            // Inlines SetSelectedNode's deselect-previous/select/auto-expand logic instead of
            // calling it directly: SetSelectedNode fires SelectedItemChanged.InvokeAsync without
            // awaiting it, so an exception thrown by a subscriber (e.g. a NavigateTo call) would
            // be silently swallowed as an unobserved task instead of surfacing here.
            if (item.Selected)
            {
                if (_selectedNode is not null && _selectedNode != item)
                    _selectedNode.SetSelected(false);
                _selectedNode = item;
                SelectedItem = item.Item;
                _selectedItem = SelectedItem;
                if (SelectedItemChanged.HasDelegate)
                    await SelectedItemChanged.InvokeAsync(SelectedItem);
                if (!(item.Expanded || (item.ParentNode?.Expanded ?? true)))
                {
                    foreach (var parent in findCollapsedParents(item))
                        parent.SetExpanded(true);
                }
            }
            else if (_selectedNode == item)
            {
                _selectedNode = null;
                SelectedItem = null;
                _selectedItem = null;
                if (SelectedItemChanged.HasDelegate)
                    await SelectedItemChanged.InvokeAsync(null);
            }
            await InvokeAsync(StateHasChanged);
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

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => _ = InvokeAsync(async () =>
            {
                await HandleSourceCollectionChangedAsync(sender, e, null);
                StateHasChanged();
            });

        /// <summary>
        /// Clears the selection when the selected item was removed from an observed source collection
        /// (the root <see cref="Items"/> or the <see cref="Children"/> collection of <paramref name="ownerNode"/>).
        /// </summary>
        internal async Task HandleSourceCollectionChangedAsync(object? sender, NotifyCollectionChangedEventArgs e, SayehTreeViewItem<TItem>? ownerNode)
        {
            if (_selectedItem is null || (_movingItem is not null && ReferenceEquals(_movingItem, _selectedItem)))
                return;

            var selectedRemoved = e.Action switch
            {
                NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace
                    => e.OldItems?.Cast<object>().Any(o => ReferenceEquals(o, _selectedItem)) == true,
                // Reset carries no OldItems: the selection was lost if the selected node was rendered
                // from this collection and the collection no longer contains it.
                NotifyCollectionChangedAction.Reset
                    => _selectedNode is not null && _selectedNode.ParentNode == ownerNode
                       && sender is IEnumerable<TItem> source && !source.Contains(_selectedItem),
                _ => false
            };
            if (!selectedRemoved)
                return;

            _selectedNode = null;
            SelectedItem = null;
            _selectedItem = null;
            if (SelectedItemChanged.HasDelegate)
                await SelectedItemChanged.InvokeAsync(null);
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
            ArgumentNullException.ThrowIfNull(treeItem.Id);
            var item = treeItem.Item!;
            _allItems[treeItem.Id] = treeItem;
            treeItem.ParentNode?.Register(treeItem);
            // A selected item re-created elsewhere (e.g. moved by drag and drop) becomes the selected node again.
            if (_selectedItem is not null && ReferenceEquals(item, _selectedItem))
                _selectedNode = treeItem;
        }

        internal void Unregister(SayehTreeViewItem<TItem> treeItem)
        {
            ArgumentNullException.ThrowIfNull(treeItem);
            ArgumentNullException.ThrowIfNull(treeItem.Id);
            var item = treeItem.Item!;
            _allItems.Remove(treeItem.Id);
            treeItem.ParentNode?.Unregister(treeItem);
            if (_selectedNode == treeItem)
                _selectedNode = null;
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
            if (node.Expanded || (node.ParentNode?.Expanded ?? true))
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
            if (node.ParentNode is null)
                return parents;
            if (!node.ParentNode.Expanded)
            {
                parents.Push(node.ParentNode);
                foreach (var pr in findCollapsedParents(node.ParentNode).Reverse())
                    parents.Push(pr);
            }
            return parents;
        }

        private ValueTask DisplaySelectedItem(TItem? selectedItem)
        {
            _selectedItem = selectedItem;
            if (selectedItem is null)
                return ValueTask.CompletedTask;

            if (this.Parent is not null)
               return DisplaySelectedItemBasedOnItems(selectedItem);
            else if (!Virtualize)
            {
                var node = GetNode(selectedItem);
                if (node is not null)
                {
                    SetSelectedNode(node);
                }
            }
            return ValueTask.CompletedTask;
        }

        private async ValueTask DisplaySelectedItemBasedOnItems(TItem? selectedItem) {
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
                        if (lastParent._children.Count == 0 && Children is not null)
                        {
                            if (Children.Invoke(firtParent).Count() > 0)
                                await Task.Delay(100);
                        }
                        var parent = lastParent._children.FirstOrDefault(f => f.Value.Item == item);
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
                        _selectedNode?.SetSelected(true);
                    }

                }
            }
        }

        private Stack<TItem> findParents(TItem item)
        {
            ArgumentNullException.ThrowIfNull(Parent);
            var parents = new Stack<TItem>();
            var parent = Parent(item);
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

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_dragDropHandle is not null)
                {
                    await _dragDropHandle.InvokeVoidAsync("stop");
                    await _dragDropHandle.DisposeAsync();
                }
                if (_module is not null)
                    await _module.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException || ex is OperationCanceledException)
            {
                // The JS side may already be gone if the client disconnected.
            }
            _dragDropHandle = null;
            _module = null;
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
                _dotNetRef?.Dispose();
                _currentSelectedChangedDebounce?.Dispose();
                if (_observedItems is not null)
                    _observedItems.CollectionChanged -= OnItemsCollectionChanged;
                _observedItems = null;
                _allItems.Clear();
            }

            _disposed = true;
        }


        internal string GetKeyForItem(TItem item)
        {
            if (item is null)
                return Guid.NewGuid().ToString("N");

            return _generatedKeys.GetValue(item, _ => Guid.NewGuid().ToString("N"));
        }

        internal bool IsItemDraggable(TItem item) => AllowDragDrop && (CanDrag?.Invoke(item) ?? true);

        /// <summary>
        /// Builds the drop description for the nodes rendered with the given ids, or <c>null</c> when the drop is invalid
        /// (unknown node, undraggable item, or the target is the dragged node itself or one of its descendants).
        /// </summary>
        internal TreeViewItemDropEventArgs<TItem>? CreateDropArgs(string sourceId, string targetId, string position)
        {
            if (!AllowDragDrop
                || !Enum.TryParse<TreeViewDropPosition>(position, out var dropPosition)
                || !_allItems.TryGetValue(sourceId, out var source) || source.Item is null
                || !_allItems.TryGetValue(targetId, out var target) || target.Item is null
                || !IsItemDraggable(source.Item))
                return null;
            for (var node = target; node is not null; node = node.ParentNode)
            {
                if (node == source)
                    return null;
            }

            var item = source.Item;
            var newParent = dropPosition == TreeViewDropPosition.Inside ? target.Item : target.ParentNode?.Item;
            var siblings = (newParent is null ? Items : Children?.Invoke(newParent))?.ToList();
            var index = -1;
            if (siblings is not null)
            {
                var oldIndex = siblings.FindIndex(s => ReferenceEquals(s, item));
                if (dropPosition == TreeViewDropPosition.Inside)
                    index = siblings.Count;
                else
                {
                    index = siblings.FindIndex(s => ReferenceEquals(s, target.Item));
                    if (index >= 0 && dropPosition == TreeViewDropPosition.After)
                        index++;
                }
                // The index is meant to be used after the item was removed from its old position.
                if (oldIndex >= 0 && index > oldIndex)
                    index--;
            }
            return new TreeViewItemDropEventArgs<TItem>(item, target.Item, dropPosition, source.ParentNode?.Item, newParent, index);
        }

        /// <summary>
        /// A drop is allowed when <see cref="CanDrop"/> and <see cref="ItemDroppedCommand"/>'s CanExecute both accept it.
        /// </summary>
        bool IsDropAllowed(TreeViewItemDropEventArgs<TItem> args)
            => (CanDrop?.Invoke(args) ?? true) && (ItemDroppedCommand?.CanExecute(args) ?? true);

        /// <summary>
        /// Called by the JS module when the dragged item enters <paramref name="targetId"/>'s node: returns every
        /// <see cref="TreeViewDropPosition"/> (as its name) the item may be dropped at on that node.
        /// </summary>
        [JSInvokable]
        public async Task<string[]> GetAllowedDropPositionsAsync(string sourceId, string targetId)
        {
            if (!_allItems.TryGetValue(sourceId, out var source) || source.Item is null
                || !_allItems.TryGetValue(targetId, out var target) || target.Item is null)
                return [];

            var args = new TreeViewItemDragEnterEventArgs<TItem>(source.Item, target.Item);
            await OnDragEntering(args);
            if (args.Cancel)
                return [];

            args.AllowedPositions = Enum.GetValues<TreeViewDropPosition>()
                .Where(position => CreateDropArgs(sourceId, targetId, position.ToString()) is { } dropArgs && IsDropAllowed(dropArgs))
                .ToArray();
            await OnDragEntered(args);
            return args.AllowedPositions.Select(p => p.ToString()).ToArray();
        }

        [JSInvokable]
        public async Task DropItemAsync(string sourceId, string targetId, string position)
        {
            var args = CreateDropArgs(sourceId, targetId, position);
            if (args is null || !IsDropAllowed(args))
                return;

            await OnDropping(args);
            if (args.Cancel)
                return;

            // OnDropped is inside the guard as well: a derived tree may move the item there.
            _movingItem = args.Item;
            try
            {
                ItemDroppedCommand?.Execute(args);
                if (ItemDropped.HasDelegate)
                    await ItemDropped.InvokeAsync(args);

                // Reveal the dropped item when it was moved into a collapsed node.
                if (args.Position == TreeViewDropPosition.Inside && _allItems.TryGetValue(targetId, out var target) && !target.Expanded)
                    target.SetExpanded(true);
                await OnDropped(args);
            }
            finally
            {
                _movingItem = null;
            }
            StateHasChanged();
        }

        /// <summary>
        /// Called the first time the dragged item enters a node during a drag, before the drop positions are evaluated.
        /// Set <see cref="TreeViewItemDragEnterEventArgs{TItem}.Cancel"/> to refuse every drop on that node.
        /// </summary>
        protected virtual Task OnDragEntering(TreeViewItemDragEnterEventArgs<TItem> args) => Task.CompletedTask;

        /// <summary>
        /// Called after the drop positions allowed on the entered node were evaluated
        /// (see <see cref="TreeViewItemDragEnterEventArgs{TItem}.AllowedPositions"/>). Not called when OnDragEntering cancelled.
        /// </summary>
        protected virtual Task OnDragEntered(TreeViewItemDragEnterEventArgs<TItem> args) => Task.CompletedTask;

        /// <summary>
        /// Called when an allowed drop happens, before <see cref="ItemDroppedCommand"/> and <see cref="ItemDropped"/>.
        /// Set <see cref="TreeViewItemDropEventArgs{TItem}.Cancel"/> to cancel the drop (e.g. after a confirmation dialog).
        /// </summary>
        protected virtual Task OnDropping(TreeViewItemDropEventArgs<TItem> args) => Task.CompletedTask;

        /// <summary>
        /// Called after <see cref="ItemDroppedCommand"/> and <see cref="ItemDropped"/> handled the drop. Not called when the drop was cancelled.
        /// </summary>
        protected virtual Task OnDropped(TreeViewItemDropEventArgs<TItem> args) => Task.CompletedTask;

        /// <summary>
        /// Whether a derived tree overrides <see cref="OnDragEntering"/> or <see cref="OnDragEntered"/>: the JS module must then
        /// ask .NET for the allowed positions on every entered node even without CanDrop/ItemDroppedCommand.
        /// </summary>
        internal bool OverridesDragEnter
            => _overridesDragEnter ??= IsOverridden(nameof(OnDragEntering)) || IsOverridden(nameof(OnDragEntered));
        bool? _overridesDragEnter;

        bool IsOverridden(string methodName)
            => GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)?.DeclaringType != typeof(SayehTreeView<TItem>);

        #endregion

        #region API

        public SayehTreeViewItem<TItem> GetNode(TItem item) => _allItems.FirstOrDefault(f => f.Value.Item == item).Value;

        #endregion

    }
}
