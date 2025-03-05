using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehDataGridRow<TItem>
    {

        #region Fields

        Nullable<bool> _implementedIEditableObject;
        List<ValidationResult>? _validationResult;
        ValidationContext? _validationContext;
        SayehDataGridCell<TItem>? _currentEditCell;

        internal DataGridItemMode Mode { get; set; } = DataGridItemMode.Readonly;


        #endregion

        #region Parameters



        #endregion

        #region Functions

        [RequiresUnreferencedCode("Use 'MethodFriendlyToTrimming' instead", Url = "http://help/unreferencedcode")]
        internal bool BeginEdit()
        {
            if (!_implementedIEditableObject.HasValue)
                _implementedIEditableObject = typeof(IEditableObject).IsAssignableFrom(typeof(TItem));
            if (Columns is null || Item is null)
                return false;
            if (Mode == DataGridItemMode.Readonly && Owner.Grid.BeginItemEdit(this))
            {
                _validationContext = new ValidationContext(Item);
                _validationResult = new List<ValidationResult>();
                Mode = DataGridItemMode.Edit;
                foreach (var col in Columns.Where(w => w.IsEditable))
                {
                    ColBeginEdit(col);
                }
                Columns.First().SetFocuse();
                Owner.Grid.setCellEditableConfig(this);
            }
            else
                return false;
            return true;
        }

        internal void EndEdit()
        {
            if (_implementedIEditableObject!.Value)
                ((IEditableObject)Item!).EndEdit();
            Mode = DataGridItemMode.Readonly;
            _currentEditCell = null;
            StateHasChanged();
            Owner.Grid.removeCellEditableConfig(this);
        }

        [RequiresUnreferencedCode("Use 'MethodFriendlyToTrimming' instead", Url = "http://help/unreferencedcode")]
        internal async Task<bool> CanCommit()
        {
            if (Columns is null || Item is null)
                return true;
            if (Mode == DataGridItemMode.Readonly)
                return false;
            await Task.Delay(10);
            if (!CommitCell())
                return false;
            CommitEdit();
            _validationContext!.MemberName = null;
            return Validator.TryValidateObject(Item!, _validationContext!, _validationResult!);
        }

        [RequiresUnreferencedCode("Use 'MethodFriendlyToTrimming' instead", Url = "http://help/unreferencedcode")]
        private bool CommitCell()
        {
            if (_currentEditCell is null)
                return true;
            var col = Columns!.ElementAt(_currentEditCell.ColumnIndex - 1);
            if (!col.IsEditable)
                return true;
            var cprvcol = ((IEditableColumn<TItem>)col);
            if (cprvcol is not null)
            {
                _validationContext!.MemberName = cprvcol.GetEditPropertyPath();
                if (string.IsNullOrEmpty(_validationContext!.MemberName))
                    return true;
                //if property does not have valide data or event has Cancel == true, prevent cell change and set focuse to previouse cell
                if (!Validator.TryValidateProperty(cprvcol.GetCurrentValue(), _validationContext, _validationResult)
                    || !Owner.Grid.EditEndingForCell(Item!, cprvcol.GetEditPropertyPath(), cprvcol.GetCurrentValue(), EditActionEnum.Commit))
                {
                    col.SetFocuse();
                    return false;
                }
                else
                {
                    cprvcol.UpdateSource();
                    Owner.Grid.EditEndedForCell(Item!, cprvcol.GetEditPropertyPath(), EditActionEnum.Commit);
                }

            }
            return true;
        }

        private void ColBeginEdit(SayehColumnBase<TItem> col)
        {
            if (!col.IsEditable)
                return;
            var cCol = ((IEditableColumn<TItem>)col);
            if (cCol is null)
                return;
            cCol.BeginEdit(Item!);
            if (!cCol.IsReadonly)
                col.InternalIsReadonly = !Owner.Grid.BeginPropertyEdit(Item!, cCol.GetEditPropertyPath());
            else
                col.InternalIsReadonly = true;
        }

        private void CommitEdit()
        {
            if (Columns is null)
                return;
            //foreach (var col in Columns.Where(w => w.IsEditable).Cast<IEditableColumn<TGridItem>>())
            //{
            //    col.UpdateSource();
            //}
            StateHasChanged();
        }

        internal async void CancelEdit()
        {
            if (_implementedIEditableObject!.Value)
                ((IEditableObject)Item!).CancelEdit();
            if (_currentEditCell is not null)
            {
                var col = Columns!.ElementAt(_currentEditCell.ColumnIndex - 1);
                if (col.IsEditable)
                {
                    var cCol = ((IEditableColumn<TItem>)col);
                    Owner.Grid.EditEndedForCell(Item!, cCol.GetEditPropertyPath(), EditActionEnum.Cancel);
                }

                _currentEditCell = null;
            }
            if (Mode == DataGridItemMode.Edit)
                Mode = DataGridItemMode.Readonly;
            await Owner.Grid.EndEdit(this, EditActionEnum.Cancel);
            Owner.Grid.removeCellEditableConfig(this);
            StateHasChanged();
        }

        [RequiresUnreferencedCode("Use 'MethodFriendlyToTrimming' instead", Url = "http://help/unreferencedcode")]
        private void OnRowDblClicked()
        {
            if (Owner.Grid.IsReadonly)
                return;
            BeginEdit();
        }

        #endregion

    }
}
