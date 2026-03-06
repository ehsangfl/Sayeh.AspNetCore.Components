using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
public interface IEditableColumn<TItem> where TItem : class
{
    /// <summary>
    /// Indicates whether the data should be readonly by this column.
    ///
    /// The default value may vary according to the column type (Editable columns must implement <see cref="IEditableColumn{TGridItem}" />
    /// </summary>
    [Parameter]
    public bool IsReadonly { get; set; }


    /// <summary>
    /// get value entered by user before commit edit
    /// </summary>
    /// <returns></returns>
    public object? GetCurrentValue();

    /// <summary>
    /// prepare column for edit. for example get initial value of column or ...
    /// this function called on begin edit
    /// </summary>
    /// <param name="Item"></param>
    public void BeginEdit(TItem Item);

    /// <summary>
    /// Update source value from input value. this funcation called on commit edit
    /// </summary>
    public void UpdateSource();

    /// <summary>
    /// Cancel edit
    /// </summary>
    public void CancelEdit();

    /// <summary>
    /// get name of property witch column would to edit
    /// </summary>
    public string? GetEditPropertyPath();

    /// <summary>
    /// Render edit mode for column
    /// </summary>
    /// <param name="builder"></param>
    public void CellEditContent(RenderTreeBuilder builder);        

}
