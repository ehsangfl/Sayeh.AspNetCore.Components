using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components;

partial class SayehDataGridCell<TItem>
{

    public string? ErrorMessage { get; set; }

	#region Functions

	public void MakeValid()
	{
		this.Class ??= string.Empty;
		this.Class.Replace("invalid", "");
        ErrorMessage = null;
        StateHasChanged();
	}

    public void MakeInvalid(string Error)
    {
        this.Class ??= string.Empty;
        this.Class += " invalid ";
        ErrorMessage = Error;
        StateHasChanged();
    }

    #endregion

}
