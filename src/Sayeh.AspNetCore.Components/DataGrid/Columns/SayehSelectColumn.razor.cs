using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Sayeh.AspNetCore.Components.Infrastructure;
using System.Reflection;

namespace Sayeh.AspNetCore.Components
{
    public partial class SayehSelectColumn<TItem> : SayehPropertyColumn<TItem, bool> where TItem : class
    {

        #region Fields

        private PropertyInfo? _propertyInfo;

        private int _selectedItemsCount = 0;

        private Nullable<bool> selectAll { get; set; } = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether the [All] checkbox is disabled (not clickable).
        /// </summary>
        [Parameter]
        public bool SelectAllDisabled { get; set; } = false;

        ///// <summary>
        ///// Gets or sets the template for the [All] checkbox column template.
        ///// </summary>
        //[Parameter]
        //public RenderFragment<SelectAllTemplateArgs>? SelectAllTemplate { get; set; }

        /// <summary>
        /// Gets or sets the function executed to determine if the item can be selected.
        /// </summary>
        [Parameter]
        public Func<TItem, bool>? Selectable { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets a callback when list of selected items changed.
        /// </summary>
        [Parameter]
        public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed when the row is selected or unselected.
        /// This action is required to update you data model.
        /// </summary>
        [Parameter]
        public EventCallback<(TItem Item, bool Selected)> OnSelect { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed when the [All] checkbox is clicked.
        /// When this action is defined, the [All] checkbox is displayed.
        /// This action is required to update you data model.
        /// </summary>
        [Parameter]
        public EventCallback<bool?> SelectAllChanged { get; set; }

        #endregion

        #region Initialization

        public SayehSelectColumn()
        {
            Width = "50px";
            MinWidth = "50px";
            HeaderContent = GetHeaderContent();
            IsEditable = false;
        }

        #endregion

        #region Functions

        private void UpdateChecked(bool e, TItem item)
        {
            if (PropertyInfo is not null)
                PropertyInfo.SetValue(item, e);
            if (e)
                _selectedItemsCount++;
            else if (_selectedItemsCount > 0)
                _selectedItemsCount--;
            selectAll = GetSelectAll();
            HeaderCell?.RaiseStateHasChanged();
        }

        private bool GetChecked(TItem item)
        {
            if (PropertyInfo is null)
                return false;
            return _compiledProperty?.Invoke(item) ?? false;
        }

        private RenderFragment GetHeaderContent()
        {
            selectAll = GetSelectAll();
            return new RenderFragment((builder) =>
            {
                this.GetHeaderContent(builder);
            });

        }

        /// <summary />
        private bool? GetSelectAll()
        {
            if (_selectedItemsCount == 0)
                return false;
            // Using SelectedItems only
            if (InternalGridContext != null)
            {
                if (InternalGridContext.TotalItemCount > _selectedItemsCount)
                    return null;
                else if (InternalGridContext.TotalItemCount == _selectedItemsCount)
                    return true;
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc />
        protected internal override string? RawCellContent(TItem item)
        {
            return TooltipText?.Invoke(item);
        }

        /// <summary />
        internal async Task OnCheckAllChangeAsync(bool? e)
        {
            if (Grid == null || SelectAllDisabled || Grid.Items is null)
                return;
            if (Grid.Virtualize)
            {
                foreach (var item in Grid.Items)
                    PropertyInfo?.SetValue(item, e);
                InternalGridContext.ApplySelectedItems(e == true);
            }
            else
                InternalGridContext.ApplySelectedItems(e == true);

            if (e == true)
                _selectedItemsCount = Grid._internalItemsSource?.Count() ?? 0;
            else
                _selectedItemsCount = 0;

            if (SelectAllChanged.HasDelegate)
            {
                await SelectAllChanged.InvokeAsync(e);
            }

            if ((Grid?._internalItemsSource is not null && false) && SelectedItemsChanged.HasDelegate)
            {
                await SelectedItemsChanged.InvokeAsync(getSelectedItems());
            }
            selectAll = GetSelectAll();
        }

        public IEnumerable<TItem> getSelectedItems()
            => _compiledProperty is null || Grid._internalItemsSource is null
            ? new List<TItem>()
            : Grid._internalItemsSource.Where(w => _compiledProperty.Invoke(w));

        public override void SetFocuse()
        {

        }

        #endregion

    }
}
