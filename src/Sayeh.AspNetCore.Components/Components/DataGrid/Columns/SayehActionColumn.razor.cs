using Microsoft.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehActionColumn<TItem> : SayehColumnBase<TItem> where TItem : class
    {

        public SayehActionColumn()
        {
            this.IsEditable = false;
        }

        /// <summary>
        /// Specifies the row actions to be rendered for each row (typically a handful of icon buttons or
        /// <see cref="SayehCommandColumn{TItem}"/> command links).
        /// </summary>
        [Parameter] public RenderFragment<TItem> ChildContent { get; set; } = _ => __builder => { };

        /// <summary>
        /// When true (the default), the actions stay hidden until the row is hovered or focused - the
        /// same reveal behavior added to <c>SayehTreeViewItem.Actions</c>. Set to false to keep the
        /// actions always visible.
        /// </summary>
        [Parameter] public bool RevealOnHover { get; set; } = true;

        public override void SetFocuse()
        {

        }

    }
}
