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
    private SayehDataGridCell<TItem> selectCell;

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
    private InternalGridContext<TItem> Owner { get; set; } = default!;

    [Parameter]
    public IEnumerable<SayehColumnBase<TItem>>? Columns { get; set; }

    private DataGridRowOptions<TItem>? _options;
    [Parameter]
    public DataGridRowOptions<TItem>? Options { get; set; }


    #endregion

    #region Initialization

    protected override void OnInitialized()
    {
        RowId = $"r{Owner.GetNextRowId()}";
        Owner.Register(this);
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
        }
    }

    #endregion

    protected string? ClassValue => new CssBuilder(Class)
        .AddClass("hover", when: Owner.Grid.ShowHover)
        .Build();

    protected string? StyleValue => new StyleBuilder(Style)
       .AddStyle("height", $"{Owner.Grid.ItemSize:0}px", () => Owner.Grid.Virtualize && RowType == DataGridRowType.Default)
       .AddStyle("height", "100%", () => (!Owner.Grid.Virtualize || Owner.Rows.Count == 0) && Owner.Grid.Loading && RowType == DataGridRowType.Default)
       .AddStyle("align-items", "center", () => Owner.Grid.Virtualize && RowType == DataGridRowType.Default && string.IsNullOrEmpty(Style))
       .Build();

   
    public void Dispose() => Owner.Unregister(this);

    internal void Register(SayehDataGridCell<TItem> cell)
    {
        cell.CellId = $"c{Owner.GetNextCellId()}";
        cells.Add(cell.CellId, cell);
    }

    internal void setSelected(bool selected)
    {
        selectCell ??= this.cells.FirstOrDefault(f => f.Value.Column is SayehSelectColumn<TItem>).Value;
        selectCell?.setSelected(selected);
    }

    internal void Unregister(SayehDataGridCell<TItem> cell)
    {
        cells.Remove(cell.CellId!);
        cell.Dispose();
    }

    internal async Task HandleOnRowFocusAsync()
    {
       await HandleOnRowClickAsync(Id);
    }

    private async Task HandleOnCellFocusAsync()
    {
        //var cellId = args.CellId;
        //if (_currentEditCell is not null && cellId is not null && cellId.Equals(_currentEditCell.CellId))
        //{
        //    Console.WriteLine("enter to selected cell");
        //    return;
        //}
        //if (cells.TryGetValue(cellId!, out var cell))
        //{
        //    await OnCellFocus.InvokeAsync(cell);
        //    var col = this.Columns?.ElementAt(cell.GridColumn - 1);
        //    if (col is not null && _currentEditCell is not null && !cell.CellId.Equals(_currentEditCell.CellId) && CommitCell())
        //    {
        //        _currentEditCell = cell;
        //        ColBeginEdit(col);
        //        col.SetFocuse();
        //    }
        //    else if (col is not null && _currentEditCell is null)
        //    {
        //        _currentEditCell = cell;
        //        ColBeginEdit(col);
        //        col.SetFocuse();
        //    }
        //}
    }

    /// <summary />
    internal async Task HandleOnRowClickAsync(string rowId)
    {
        if (Owner.Rows.TryGetValue(rowId, out var row))
        {
            if (Owner.Grid.OnRowClick.HasDelegate)
            {
                await Owner.Grid.OnRowClick.InvokeAsync(row);
            }
        }
    }

    /// <summary />
    internal async Task HandleOnRowDoubleClickAsync(string rowId)
    {
        if (Owner.Rows.TryGetValue(rowId, out var row))
        {
            if (Owner.Grid.OnRowDoubleClick.HasDelegate)
            {
                await Owner.Grid.OnRowDoubleClick.InvokeAsync(row);
            }
        }
    }

    /// <summary />
    internal void HandleOnRowKeyDown(string rowId, KeyboardEventArgs e)
    {
        if (e.Key.ToLower() == "f2")
            BeginEdit();
    }

    private static string? ColumnClass(SayehColumnBase<TItem> column) => column.Align switch
    {
        Align.Center => $"col-justify-center {column.Class}",
        Align.End => $"col-justify-end {column.Class}",
        _ => column.Class,
    };

    /// <summary />
    Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
        => callback.InvokeAsync(arg);
}
