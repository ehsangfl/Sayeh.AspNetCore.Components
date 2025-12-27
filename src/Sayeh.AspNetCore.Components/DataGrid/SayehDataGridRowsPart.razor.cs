using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components;

partial class SayehDataGridRowsPart
{
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    public void ReRender() {
        //InvokeAsync(StateHasChanged);
    }
}
