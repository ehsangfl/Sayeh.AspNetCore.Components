namespace Sayeh.AspNetCore.Components
{
    /// <summary>
    /// Where a dragged item is dropped relative to the <see cref="TreeViewItemDropEventArgs{TItem}.Target"/> item.
    /// </summary>
    public enum TreeViewDropPosition
    {
        /// <summary>As the previous sibling of the target.</summary>
        Before,
        /// <summary>As the last child of the target.</summary>
        Inside,
        /// <summary>As the next sibling of the target.</summary>
        After
    }

    /// <summary>
    /// Describes a drag and drop operation in <see cref="SayehTreeView{TItem}"/>.
    /// The tree does not own its data, so the handler is responsible for moving <see cref="Item"/>
    /// in the underlying collections (observable collections re-render the tree automatically).
    /// </summary>
    public class TreeViewItemDropEventArgs<TItem> where TItem : class
    {
        public TreeViewItemDropEventArgs(TItem item, TItem target, TreeViewDropPosition position, TItem? oldParent, TItem? newParent, int index)
        {
            Item = item;
            Target = target;
            Position = position;
            OldParent = oldParent;
            NewParent = newParent;
            Index = index;
        }

        /// <summary>The dragged item.</summary>
        public TItem Item { get; }

        /// <summary>The item the dragged item was dropped on.</summary>
        public TItem Target { get; }

        public TreeViewDropPosition Position { get; }

        /// <summary>The current parent of <see cref="Item"/>; <c>null</c> when it is a root item.</summary>
        public TItem? OldParent { get; }

        /// <summary>The parent <see cref="Item"/> should be moved to; <c>null</c> means the root <see cref="SayehTreeView{TItem}.Items"/>.</summary>
        public TItem? NewParent { get; }

        /// <summary>
        /// The index at which <see cref="Item"/> should be inserted into the children of <see cref="NewParent"/>,
        /// computed as if <see cref="Item"/> had already been removed from its old position.
        /// -1 when the sibling collection is unknown (e.g. when the tree is built with ChildContent).
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Set to <c>true</c> in <see cref="SayehTreeView{TItem}.OnDropping"/> to cancel the drop:
        /// neither <see cref="SayehTreeView{TItem}.ItemDroppedCommand"/> nor <see cref="SayehTreeView{TItem}.ItemDropped"/> is then raised.
        /// </summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Describes the dragged item entering a node of <see cref="SayehTreeView{TItem}"/>
    /// (see <see cref="SayehTreeView{TItem}.OnDragEntering"/> and <see cref="SayehTreeView{TItem}.OnDragEntered"/>).
    /// </summary>
    public class TreeViewItemDragEnterEventArgs<TItem> where TItem : class
    {
        public TreeViewItemDragEnterEventArgs(TItem item, TItem target)
        {
            Item = item;
            Target = target;
        }

        /// <summary>The dragged item.</summary>
        public TItem Item { get; }

        /// <summary>The item whose node the dragged item entered.</summary>
        public TItem Target { get; }

        /// <summary>
        /// The positions the item may be dropped at on <see cref="Target"/>. Empty in OnDragEntering (not evaluated yet);
        /// in OnDragEntered it holds the positions accepted by the tree, CanDrop and ItemDroppedCommand.CanExecute.
        /// </summary>
        public IReadOnlyCollection<TreeViewDropPosition> AllowedPositions { get; internal set; } = [];

        /// <summary>
        /// Set to <c>true</c> in OnDragEntering to refuse every drop on <see cref="Target"/> for the current drag.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
