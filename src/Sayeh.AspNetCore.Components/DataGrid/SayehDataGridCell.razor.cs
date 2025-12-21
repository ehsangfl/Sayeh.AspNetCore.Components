using Microsoft.AspNetCore.Components;
using Sayeh.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Utilities;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components;

public partial class SayehDataGridCell<TItem> : FluentComponentBase where TItem : class
{

    #region Fields

    internal string CellId { get; set; } = string.Empty;

    private bool _isSelected { get; set; }

    #endregion

    private DataGridCellOptions<TItem>? _options;
    [Parameter]
    public DataGridCellOptions<TItem>? Options { get; set; }

    /// <summary>
    /// Gets or sets the reference to the item that holds this cell's values.
    /// </summary>
    [Parameter]
    public TItem? Item { get; set; }

    /// <summary>
    /// Gets or sets the cell type. See <see cref="DataGridCellType"/>.
    /// </summary>
    [Parameter]
    public DataGridCellType? CellType { get; set; } = DataGridCellType.Default;

    /// <summary>
    /// Gets or sets the column index of the cell.
    /// This will be applied to the css grid-column-index value applied to the cell.
    /// </summary>
    [Parameter]
    public int ColumnIndex { get; set; }

    /// <summary>
    /// Gets a reference to the column that this cell belongs to.
    /// </summary>
    public SayehColumnBase<TItem>? Column => GridContext.Grid._columns.ElementAtOrDefault(ColumnIndex);

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the owning <see cref="SayehDataGridRow{TItem}"/> component.
    /// </summary>
    [CascadingParameter(Name = "OwningRow")]
    public SayehDataGridRow<TItem> Owner { get; set; } = default!;

    /// <summary>
    /// Gets a reference to the enclosing <see cref="FluentDataGrid{TItem}" />.
    /// </summary>
    protected SayehDataGrid<TItem> Grid => GridContext.Grid;

    /// <summary>
    /// Gets or sets the owning <see cref="FluentDataGrid{TItem}"/> component
    /// </summary>
    [CascadingParameter]
    private InternalGridContext<TItem> GridContext { get; set; } = default!;

    protected override void OnParametersSet()
    {
        if (_options != Options)
        {
            _options = Options;
            Item = _options?.Item;
            Style = _options?.Style;
            ColumnIndex = _options?.ColumnIndex ?? 0;
            Class = _options?.Class;
        }
        base.OnParametersSet();
    }

    protected string? ClassValue => new CssBuilder(Class)
        .AddClass("column-header", when: CellType == DataGridCellType.ColumnHeader)
        .AddClass("select-all", when: CellType == DataGridCellType.ColumnHeader && Column is SayehSelectColumn<TItem>)
        .AddClass("multiline-text", when: Grid.MultiLine && (Grid.Items is not null || Grid.ItemsProvider is not null) && CellType != DataGridCellType.ColumnHeader)
        .AddClass(Owner.Class)
        .Build();

    protected string? StyleValue => new StyleBuilder(Style)
        .AddStyle("grid-column", (ColumnIndex+1).ToString(), () => (Grid.Items is not null) || Grid.Virtualize)
        .AddStyle("text-align", "center", Column is SelectColumn<TItem>)
        .AddStyle("align-content", "center", Column is SelectColumn<TItem>)
        .AddStyle("padding-inline-start", "calc(((var(--design-unit)* 3) + var(--focus-stroke-width) - var(--stroke-width))* 1px)", Column is SelectColumn<TItem> && Owner.RowType == DataGridRowType.Default)
        .AddStyle("padding-top", "calc(var(--design-unit) * 2.5px)", Column is SelectColumn<TItem> && (Grid.RowSize == DataGridRowSize.Medium || Owner.RowType == DataGridRowType.Header))
        .AddStyle("padding-top", "calc(var(--design-unit) * 1.5px)", Column is SelectColumn<TItem> && Grid.RowSize == DataGridRowSize.Small && Owner.RowType == DataGridRowType.Default)
        .AddStyle("width", Column?.Width, !string.IsNullOrEmpty(Column?.Width) && Grid.DisplayMode == DataGridDisplayMode.Table)
        .AddStyle("height", $"{Grid.ItemSize:0}px", () => Grid.Virtualize && Owner.RowType == DataGridRowType.Default)
        .AddStyle("height", $"{(int)Grid.RowSize}px", () => !Grid.Virtualize && !Grid.MultiLine && (Grid.Items is not null || Grid.ItemsProvider is not null))
        .AddStyle("height", "100%", Grid.MultiLine)
        .AddStyle("min-height", "44px", Owner.RowType != DataGridRowType.Default)
        .AddStyle("display", "flex", ShouldHaveDisplayFlex())
        .AddStyle("z-index", (Grid._columns.Count + 2 - this.ColumnIndex).ToString(), CellType == DataGridCellType.ColumnHeader && Grid._columns.Count > 0)
        .AddStyle(Owner.Style)
        .AddStyle(Column?.Style)
        .Build();

    //protected override void OnInitialized()
    //{
    //    Owner.Register(this);
    //}

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            Owner.Register(this);
        base.OnAfterRender(firstRender);
    }

    public void Dispose() => Owner.Unregister(this);

    internal async Task HandleOnCellKeyDownAsync(KeyboardEventArgs e)
    {
        if (!SelectColumn<TItem>.KEYBOARD_SELECT_KEYS.Contains(e.Code))
        {
            return;
        }

        if (Column != null)
        {
            await Column.OnCellKeyDownAsync(this, e);
        }
    }

    internal void HandleOnCellClickAsync()
    {
        _isSelected = true;
        Owner.CurrentCell = this;
    }

    internal async Task HandleOnCellFocusAsync()
    {
        if (CellType == DataGridCellType.Default)
        {
            await Grid.OnCellFocus.InvokeAsync(this);
        }
    }

    private bool ShouldHaveDisplayFlex()
    {

        var isHeaderCell = CellType == DataGridCellType.ColumnHeader;

        if (!isHeaderCell)
        {
            return false;
        }

        var isResizable = Grid.ResizableColumns;
        var isNotResizableWithOptions = !Grid.ResizableColumns && Column?.ColumnOptions is not null;
        var isSelectColumn = Column?.GetType() == typeof(SelectColumn<TItem>);
        //var isColumnNull = Column is null;
        var isSortable = (Column?.Sortable.HasValue ?? false) && Column?.Sortable.Value == true;
        var isColumnsCountGreaterThanZero = Grid._columns.Count > 0;

        return isHeaderCell && (isResizable || isNotResizableWithOptions) && !isSelectColumn && isColumnsCountGreaterThanZero && (!isSortable);
    }

    internal void setSelected(bool selected)
    {
        if (Item is null)
            return;
        _isSelected = selected;
        ((SayehSelectColumn<TItem>)Column!)?.PropertyInfo?.SetValue(Item, selected);
        StateHasChanged();
    }

    internal void RaiseStateHasChanged()
    => StateHasChanged();

}
