using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.AspNetCore.Components;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components
{
   public abstract partial class SayehColumnBase<TItem> where TItem : class
    {

        #region Fields

        internal bool DefaultSortApplied;

        /// <summary>
        /// Get a value indicating whether this column should act as sortable if no value was set for the
        /// <see cref="SayehColumnBase{TItem}.Sortable" /> parameter. The default behavior is not to be
        /// sortable unless <see cref="SayehColumnBase{Titem}.Sortable" /> is true.
        ///
        /// Derived components may override this to implement alternative default sortability rules.
        /// </summary>
        /// <returns>True if the column should be sortable by default, otherwise false.</returns>
        private bool? _isSortableByDefault;

        /// <summary>
        /// Get a value indicating whether this column should act as filterable if no value was set for the
        /// <see cref="SayehColumnBase{TItem}.Filterable" /> parameter. The default behavior is not to be
        /// filterable unless <see cref="SayehColumnBase{TItem}.Filterable" /> is true.
        ///
        /// Derived components may override this to implement alternative default sortability rules.
        /// </summary>
        /// <returns>True if the column should be sortable by default, otherwise false.</returns>
        private Nullable<bool> _isFilterableByDefault;

        internal SortProvider? _sortProvider;

        internal SayehDataGridCell<TItem> HeaderCell { get; set; }


        #endregion

        #region Properties

        internal bool InternalIsReadonly;

        [CascadingParameter] internal InternalGridContext<TItem> InternalGridContext { get; set; } = default!;

        /// <summary>
        /// column options component rendered dynamically. this property contains an instance of dynamic component generator. 
        /// then we can access generated component with <see cref="DynamicComponent.Instance"/>
        /// </summary>
        protected DynamicComponent? headerOptionsComponentHolder { get; set; }

        /// <summary>
        /// Title text for the column. This is rendered automatically if <see cref="HeaderCellItemTemplate" /> is not used.
        /// </summary>
        [Parameter] public string? Title { get; set; }

        /// <summary>
        /// An optional CSS class name. If specified, this is included in the class attribute of header and grid cells
        /// for this column.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Gets or sets an optional CSS style specification.
        /// If specified, this is included in the style attribute of header and grid cells
        /// for this column.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        /// <summary>
        /// If specified, controls the justification of header and grid cells for this column.
        /// </summary>
        [Parameter] public Align Align { get; set; }

        /// <summary>
        /// If true, generates a title and aria-label attribute for the cell contents
        /// </summary>
        [Parameter] public bool Tooltip { get; set; } = false;

        /// <summary>
        /// Gets or sets the value to be used as the tooltip and aria-label in this column's cells
        /// </summary>
        [Parameter] public Func<TItem, string?>? TooltipText { get; set; }

        /// <summary>
        /// An optional template for this column's header cell. If not specified, the default header template
        /// includes the <see cref="Title" /> along with any applicable sort indicators and options buttons.
        /// </summary>
        [Parameter] public RenderFragment<SayehColumnBase<TItem>>? HeaderCellItemTemplate { get; set; }

        /// <summary>
        /// If specified, indicates that this column has this associated options UI. A button to display this
        /// UI will be included in the header cell by default.
        ///
        /// If <see cref="HeaderCellItemTemplate" /> is used, it is left up to that template to render any relevant
        /// "show options" UI and invoke the grid's <see cref="SayehDataGrid{TGridItem}.ShowColumnOptions(SayehColumnBase{TGridItem})" />).
        /// </summary>
        [Parameter] public RenderFragment? ColumnOptions { get; set; }

        /// <summary>
        /// Indicates whether the data should be sortable by this column.
        ///
        /// The default value may vary according to the column type (for example, a <see cref="TemplateColumn{TItem,TValue}" />
        /// is sortable by default if any <see cref="TemplateColumn{TItem,TValue}.SortProperty" /> parameter is specified).
        /// </summary>
        [Parameter] public bool? Sortable { get; set; }

        /// <summary>
        /// Indicates whether the data should be filterable by this column.
        ///
        /// The default value may vary according to the column type (implement <see cref="IFilterableColumn{TItem, TValue}" /> filterable column
        /// </summary>
        [Parameter]
        public bool? Filterable { get; set; }

        internal bool IsEditable { get; set; }

        /// <summary>
        /// If specified and not null, indicates that this column represents the initial sort order
        /// for the grid. The supplied value controls the default sort direction.
        /// </summary>
        [Parameter]
        public ListSortDirection? DefaultSort { get; set; }

        /// <summary>
        /// If specified, virtualized grids will use this template to render cells whose data has not yet been loaded.
        /// </summary>
        [Parameter] public RenderFragment<PlaceholderContext>? PlaceholderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the width of the column.
        /// Use either this or the <see cref="SayehDataGrid{TGridItem}"/> GridTemplateColumns parameter but not both.
        /// Needs to be a valid CSS width value like '100px', '10%' or '0.5fr'.
        /// </summary>
        [Parameter] public string? Width { get; set; }

        /// <summary>
        /// Gets a reference to the enclosing <see cref="SayehDataGrid{TGridItem}" />.
        /// </summary>
        public SayehDataGrid<TItem> Grid => InternalGridContext.Grid;


        /// <summary>
        /// Gets or sets a <see cref="RenderFragment" /> that will be rendered for this column's header cell.
        /// This allows derived components to change the header output. However, derived components are then
        /// responsible for using <see cref="HeaderCellItemTemplate" /> within that new output if they want to continue
        /// respecting that option.
        /// </summary>
        protected internal RenderFragment HeaderContent { get; protected set; }

        #endregion

        #region Events

        #endregion

        #region Initialization

        /// <summary>
        /// Constructs an instance of <see cref="SayehColumnBase{TGridItem}" />.
        /// </summary>
        public SayehColumnBase()
        {
            HeaderContent = RenderDefaultHeaderContent;
            if (IsSortableByDefault())
                _sortProvider = new SortProvider();
            IsEditable = typeof(IEditableColumn<TItem>).IsAssignableFrom(GetType());
        }

        #endregion

        #region Functions

        /// <summary>
        /// Overridden by derived components to provide rendering logic for the column's cells.
        /// </summary>
        /// <param name="builder">The current <see cref="RenderTreeBuilder" />.</param>
        /// <param name="item">The data for the row being rendered.</param>
        protected internal abstract void CellContent(RenderTreeBuilder builder, TItem item);

        /// <summary>
        /// Overridden by derived components to provide the raw content for the column's cells.
        /// </summary>
        /// <param name="item">The data for the row being rendered.</param>
        protected internal virtual string? RawCellContent(TItem item) => null;

        /// <summary>
        /// Get a value indicating whether this column should act as sortable if no value was set for the
        /// <see cref="SayehColumnBase{TItem}.Sortable" /> parameter. The default behavior is not to be
        /// sortable unless <see cref="SayehColumnBase{TItem}.Sortable" /> is true.
        ///
        /// Derived components may override this to implement alternative default sortability rules.
        /// </summary>
        /// <returns>True if the column should be sortable by default, otherwise false.</returns>
        internal virtual bool IsSortableByDefault()
        {
            if (!_isSortableByDefault.HasValue)
                _isSortableByDefault = typeof(ISortableColumn<TItem>).IsAssignableFrom(this.GetType());
            return _isSortableByDefault.Value;
        }

        /// <summary>
        /// Get a value indicating whether this column is filterable if no value was set for
        /// <see cref="SayehColumnBase{TItem}.Filterable" /> parameter. The default behavior is not to be
        /// sortable unless <see cref="SayehColumnBase{TItem}.Filterable" /> is true.
        ///
        /// </summary>
        /// <returns>True if the column should be sortable by default, otherwise false.</returns>
        internal virtual bool IsFilterableByDefault()
        {
            if (!_isFilterableByDefault.HasValue)
            {
                _isFilterableByDefault = typeof(IFilterableColumn<TItem>).IsAssignableFrom(this.GetType());
            }

            return _isFilterableByDefault.Value;
        }

        /// <summary>
        /// when sort direction of any column changed,datagrid will call this function to apply new sort
        /// </summary>
        protected virtual void SortChanged(MouseEventArgs e)
        {
            if (typeof(ISortableColumn<TItem>).IsAssignableFrom(this.GetType()))
            {
                var thisSort = this as ISortableColumn<TItem>;
                if (thisSort is not null)
                {
                    if (!thisSort.SortDirection.HasValue)
                        thisSort.SortDirection = ListSortDirection.Descending;
                    thisSort.SortDirection = thisSort.SortDirection.Value switch
                    {
                        ListSortDirection.Ascending => ListSortDirection.Descending,
                        ListSortDirection.Descending => ListSortDirection.Ascending,
                        _ => ListSortDirection.Ascending
                    };
                    Grid.ApplySort(thisSort);
                }
            }
        }

        /// <summary>
        /// Close column options popup
        /// </summary>
        public virtual void CloseFilter()
        {
            if (headerOptionsComponentHolder is not null && headerOptionsComponentHolder.Instance is not null)
                ((ColumnOptionsBase<TItem>)headerOptionsComponentHolder.Instance)?.CloseDropDown();
        }

        /// <summary>
        /// set focuse to input element, on edit mode
        /// </summary>
        public abstract void SetFocuse();

        /// <summary>
        /// Event callback for when the key is pressed on a cell.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected internal virtual Task OnCellKeyDownAsync(SayehDataGridCell<TItem> cell, KeyboardEventArgs args)
        {
            return Task.CompletedTask;
        }


        #endregion

    }
}
