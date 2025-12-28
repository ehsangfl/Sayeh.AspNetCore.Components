using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components.DataGrid.Infrastructure;
using Microsoft.JSInterop;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components;

partial class SayehDataGrid<TItem>
{

    #region Parameters

    [Parameter]
    public bool IsReadonly { get; set; }

    #endregion

    #region Events

    /// <summary>
    /// raised every time user start to edit a row
    /// </summary>
    [Parameter]
    public EventCallback<DataGridBeginEditEventArgs<TItem>> ItemBeginEdit { get; set; }

    /// <summary>
    /// raised when user begin edit a cell. if user focuse on a cell with edit mode, this event raised
    /// </summary>
    [Parameter]
    public EventCallback<DataGridBeginEditEventArgs<TItem>> CellBeginEdit { get; set; }

    /// <summary>
    /// raised when user would to end edit or leave a column in edit mode
    /// </summary>
    [Parameter]
    public EventCallback<DataGridCellEditEndingEventArgs<TItem>> CellEditEnding { get; set; }

    /// <summary>
    /// raised when user ended edit or leaved a column in edit mode
    /// </summary>
    [Parameter]
    public EventCallback<DataGridCellEditEndedEventArgs<TItem>> CellEditEnded { get; set; }

    /// <summary>
    /// raised when user whould end edit a row in edit mode
    /// </summary>
    [Parameter]
    public EventCallback<DataGridRowEditEndingEventArgs<TItem>> RowEditEnding { get; set; }

    /// <summary>
    /// raised when user ended edit a row in edit mode
    /// </summary>
    [Parameter]
    public EventCallback<DataGridRowEditEndedEventArgs<TItem>> RowEditEnded { get; set; }

    /// <summary>
    /// raised when user pressed Delete button on keboard
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TItem>> RowsDelete { get; set; }

    #endregion

    #region Functions

    #region Function of events


    private DataGridBeginEditEventArgs<TItem> raiseItemBeginEdit()
    {
        var arg = new DataGridBeginEditEventArgs<TItem>(SelectedItem!);
        OnItemBeginEdit(arg);
        if (!arg.Cancel && ItemBeginEdit.HasDelegate)
            ItemBeginEdit.InvokeAsync(arg);
        return arg;
    }

    protected virtual void OnItemBeginEdit(DataGridBeginEditEventArgs<TItem> e)
    {

    }

    private DataGridBeginEditEventArgs<TItem> raiseCellBeginEdit(string propertyName)
    {
        var arg = new DataGridBeginEditEventArgs<TItem>(SelectedItem!, propertyName);
        OnCellBeginEdit(arg);
        if (!arg.Cancel && CellBeginEdit.HasDelegate)
            CellBeginEdit.InvokeAsync(arg);
        return arg;
    }

    protected virtual void OnCellBeginEdit(DataGridBeginEditEventArgs<TItem> e)
    {

    }

    private DataGridCellEditEndingEventArgs<TItem> raiseCellEditEnding(TItem Item, string? PropertyName, object? newValue, EditActionEnum EditAction)
    {
        var arg = new DataGridCellEditEndingEventArgs<TItem>(Item, PropertyName, newValue, EditAction);
        OnCellEditEnding(arg);
        if (!arg.Cancel && CellEditEnding.HasDelegate)
            CellEditEnding.InvokeAsync(arg);
        return arg;
    }

    protected virtual void OnCellEditEnding(DataGridCellEditEndingEventArgs<TItem> e)
    {

    }

    private void raiseCellEditEnded(TItem Item, string? PropertyName, EditActionEnum EditAction)
    {
        var arg = new DataGridCellEditEndedEventArgs<TItem>(Item, PropertyName, EditAction);
        OnCellEditEnded(arg);
        if (CellEditEnded.HasDelegate)
            CellEditEnded.InvokeAsync(arg);
    }

    protected virtual void OnCellEditEnded(DataGridCellEditEndedEventArgs<TItem> e)
    {

    }

    private DataGridRowEditEndingEventArgs<TItem> raiseRowEditEnding(SayehDataGridRow<TItem> row, EditActionEnum EditAction)
    {
        var arg = new DataGridRowEditEndingEventArgs<TItem>(row.Item!, EditAction);
        OnRowEditEnding(arg);
        if (!arg.Cancel && RowEditEnding.HasDelegate)
            RowEditEnding.InvokeAsync(arg);
        return arg;
    }

    protected virtual void OnRowEditEnding(DataGridRowEditEndingEventArgs<TItem> e)
    {

    }

    private void raiseRowEditEnded(SayehDataGridRow<TItem> row, EditActionEnum EditAction)
    {
        var arg = new DataGridRowEditEndedEventArgs<TItem>(row.Item!, EditAction);
        OnRowEditEnded(arg);
        if (RowEditEnded.HasDelegate)
            RowEditEnded.InvokeAsync(arg);
    }

    protected virtual void OnRowEditEnded(DataGridRowEditEndedEventArgs<TItem> e)
    {

    }

    private void raiseRowDelete()
    {
        if (RowsDelete.HasDelegate)
        {
            IEnumerable<TItem> selectedItems = new List<TItem>();

            if (_internalGridContext.SelectColumn is not null)
                selectedItems = _internalGridContext.SelectColumn.getSelectedItems();

            if (selectedItems.None() && SelectedItem is not null)
                selectedItems = new List<TItem>() { SelectedItem };
            if (selectedItems.Any())
            {
                OnRowsDelete(selectedItems);
                RowsDelete.InvokeAsync(selectedItems);
            }
        }

    }

    protected virtual void OnRowsDelete(IEnumerable<TItem> Item)
    {

    }


    #endregion

    #region EditFunctions

    internal bool BeginItemEdit(SayehDataGridRow<TItem> row)
    {
        if (_currentRow is null)
            return false;
        //row must be selected befor edit
        if (!_currentRow.RowId.Equals(row.RowId))
            return false;

        if (SelectedItem is null || row.Item is null)
            return false;
        var arg = raiseItemBeginEdit();
        if (ImplementedIEditableObject && !arg.Cancel)
            ((IEditableObject)row.Item).BeginEdit();

        return !arg.Cancel;
    }

    internal bool BeginPropertyEdit(TItem Item, string? PropertyName)
    {
        if (_currentRow is null || _currentRow.Mode == DataGridItemMode.Readonly || PropertyName is null || Item != _currentRow.Item)
            return false;
        var arg = raiseCellBeginEdit(PropertyName);
        return !arg.Cancel;
    }

    internal async Task<bool> EndEdit(SayehDataGridRow<TItem> row, EditActionEnum EditAction)
    {
        if (_currentRow is null || row is null || !row.RowId.Equals(_currentRow.RowId))
            return false;
        if (EditAction == EditActionEnum.Commit)
        {
            var canCommit = await row.CanCommit();
            if (!canCommit)
                return false;
            var arg = raiseRowEditEnding(row, EditAction);
            if (arg.Cancel)
                return false;
            _currentRow.EndEdit();
        }
        raiseRowEditEnded(row, EditAction);
        return true;
    }

    internal void CancelEdit(SayehDataGridRow<TItem> row)
    {
        if (_currentRow is null || row is null || !row.RowId.Equals(_currentRow.RowId))
            return;
        if (_currentRow.Mode == DataGridItemMode.Readonly)
            return;
        row.CancelEdit();
    }

    private async void OnKeyPress(KeyboardEventArgs e)
    {
        if (_currentRow is null || IsReadonly)
            return;
        if (e.Key == "Enter" && _currentRow.Mode == DataGridItemMode.Edit)
            await EndEdit(_currentRow, EditActionEnum.Commit);
        else if (e.Key == "Escape" && _currentRow.Mode == DataGridItemMode.Edit)
            CancelEdit(_currentRow);
        else if (e.Key == "Delete" && _currentRow is not null && _currentRow.Item is not null && _currentRow.Mode == DataGridItemMode.Readonly)
            raiseRowDelete();

    }

    internal bool EditEndingForCell(TItem Item, string? PropertyName, object? newValue, EditActionEnum EditAction)
    {
        var arg = raiseCellEditEnding(Item, PropertyName, newValue, EditAction);
        return !arg.Cancel;
    }

    internal void EditEndedForCell(TItem Item, string? PropertyName, EditActionEnum EditAction)
    {
        raiseCellEditEnded(Item, PropertyName, EditAction);
    }

    //internal void setCellEditableConfig(SayehDataGridRow<TItem> row)
    //{
    //    Module?.InvokeVoidAsync("setCellEditableConfig", _gridReference, row.RowId);
    //}

    //internal void removeCellEditableConfig(SayehDataGridRow<TItem> row)
    //{
    //    Module?.InvokeVoidAsync("removeCellEditableConfig", _gridReference, row.RowId);
    //}

    #endregion

    #endregion

}
