using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components.DataGrid.Infrastructure;
using Microsoft.FluentUI.AspNetCore.Components.Utilities;
using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using System.Security.Principal;
using System.ComponentModel.DataAnnotations;
namespace Sayeh.AspNetCore.Components;


[CascadingTypeParameter(nameof(TItem))]
public partial class SayehDataGridRow<TItem> : FluentComponentBase, IHandleEvent, IDisposable where TItem : class
{

    #region Fields

    internal string RowId { get; set; } = string.Empty;
    private readonly Dictionary<string, SayehDataGridCell<TItem>> cells = [];
    private SayehDataGridCell<TItem>? selectCell;
    private SayehDataGrid<TItem> Grid => GridContext.Grid;
    internal SayehDataGridCell<TItem>? CurrentCell { get; set; }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the reference to the item that holds this row's values.
    /// </summary>
    [Parameter]
    public TItem? Item { get; set; }

    /// <summary>
    /// Gets or sets the index of this row.
    /// When SayehDataGrid is virtualized, this value is not used.
    /// </summary>
    [Parameter]
    public int? RowIndex { get; set; }

    /// <summary>
    /// Gets or sets the string that gets applied to the css gridTemplateColumns attribute for the row.
    /// </summary>
    [Parameter]
    public string? GridTemplateColumns { get; set; } = null;

    /// <summary>
    /// Gets or sets the type of row. See <see cref="DataGridRowType"/>.
    /// </summary>
    [Parameter]
    public DataGridRowType? RowType { get; set; } = DataGridRowType.Default;

    [Parameter]
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<SayehDataGridCell<TItem>> OnCellFocus { get; set; }

    /// <summary>
    /// Gets or sets the owning <see cref="SayehDataGrid{TItem}"/> component
    /// </summary>
    [CascadingParameter]
    private InternalGridContext<TItem> GridContext { get; set; } = default!;

    [Parameter]
    public IEnumerable<SayehColumnBase<TItem>>? Columns { get; set; }

    private DataGridRowOptions<TItem>? _options;
    [Parameter]
    public DataGridRowOptions<TItem>? Options { get; set; }

    [Parameter]
    public bool ShowRowHeaders { get; set; }

    #endregion

    #region Initialization

    protected override void OnInitialized()
    {
        RowId = $"r{GridContext.GetNextRowId()}";
        GridContext.Register(this);
    }


    #endregion

    #region Functions

    internal void setRowIndex(int index)
    {
        RowIndex = index;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (_options != Options)
        {
            _options = Options;
            RowIndex = Options?.RowIndex;
            Item = Options?.Item;
            GridTemplateColumns = Options?.GridTemplateColumns;
            Columns = Options?.Columns;
            ShowRowHeaders = Options?.ShowRowHeaders ?? false;
        }
    }

    #endregion

    protected string? ClassValue => new CssBuilder(Class)
        .AddClass("hover", when: GridContext.Grid.ShowHover)
        .AddClass("edit-mode", when: Mode == DataGridItemMode.Edit)
        .Build();

    protected string? StyleValue => new StyleBuilder(Style)
       .AddStyle("height", $"{GridContext.Grid.ItemSize:0}px", () => GridContext.Grid.Virtualize && RowType == DataGridRowType.Default)
       .AddStyle("height", "100%", () => (!GridContext.Grid.Virtualize || GridContext.Rows.Count == 0) && GridContext.Grid.Loading && RowType == DataGridRowType.Default)
       .AddStyle("align-items", "center", () => GridContext.Grid.Virtualize && RowType == DataGridRowType.Default && string.IsNullOrEmpty(Style))
       .Build();


    public void Dispose() => GridContext.Unregister(this);

    internal void Register(SayehDataGridCell<TItem> cell)
    {
        cell.CellId = $"c{GridContext.GetNextCellId()}";
        cells.Add(cell.CellId, cell);
    }

    internal void setSelected(bool selected)
    {
        selectCell ??= this.cells.FirstOrDefault(f => f.Value.Column is SayehSelectColumn<TItem>).Value;
        selectCell?.setSelected(selected);
    }

    internal void Unregister(SayehDataGridCell<TItem> cell)
        => cells.Remove(cell.CellId!);

    internal async Task HandleOnRowFocusAsync()
    {
        if (Grid.OnRowFocus.HasDelegate)
        {
            await Grid.OnRowFocus.InvokeAsync(this);
        }
        await GridContext.Grid.OnRowFocusAsync(this);
    }

    internal async Task HandleOnRowDoubleClickAsync(string rowId)
    {
        if (GridContext.Rows.TryGetValue(rowId, out var row))
        {
            if (GridContext.Grid.OnRowDoubleClick.HasDelegate)
            {
                await GridContext.Grid.OnRowDoubleClick.InvokeAsync(row);
            }
        }
        OnRowDblClicked();
        //if (CurrentCell is not null)
        //{
        //    if (_currentEditCell is not null && !CurrentCell.CellId.Equals(_currentEditCell.CellId) && CommitCell())
        //        return;
        //    else
        //    {
        //        _currentEditCell = CurrentCell;
        //        ColBeginEdit(_currentEditCell.Column!);
        //        _currentEditCell.Column!.SetFocuse();
        //    }
        //}
    }

    /// <summary />
    internal async Task HandleOnRowClickAsync(string rowId)
    {
        if (GridContext.Rows.TryGetValue(rowId, out var row))
        {
            await GridContext.Grid.OnRowFocusAsync(this);
            if (GridContext.Grid.OnRowClick.HasDelegate)
            {
                await GridContext.Grid.OnRowClick.InvokeAsync(row);
            }
        }
    }

    /// <summary />
    internal void HandleOnRowKeyDown(string rowId, KeyboardEventArgs e)
    {
        if (e.Key.ToLower() == "f2")
            BeginEdit();
    }

    private static string? ColumnJustifyClass(SayehColumnBase<TItem> column)
    {
        return new CssBuilder(column.Class)
            .AddClass("col-justify-start", column.Align == Align.Start)
            .AddClass("col-justify-center", column.Align == Align.Center)
            .AddClass("col-justify-end", column.Align == Align.End)
            .Build();
    }

    internal void ReRenderHeaderRow()
    {
        if (ShowRowHeaders)
            cells.FirstOrDefault().Value?.RaiseStateHasChanged();
    }

    /// <summary />
    Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
        => callback.InvokeAsync(arg);
}
