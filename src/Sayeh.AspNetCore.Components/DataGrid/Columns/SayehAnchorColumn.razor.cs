using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehAnchorColumn<TItem>: SayehColumnBase<TItem> where TItem : class
    {

        public SayehAnchorColumn()
        {
            this.IsEditable = false;
        }

        /// <summary>
        /// Specifies the content to be rendered for each row in the table.
        /// </summary>
        [Parameter] public RenderFragment<TItem> ChildContent { get; set; } = _=> __builder => __builder.AddContent(0,"...") ;

        [Parameter] public Icon? StartIcon { get; set; } 

        [Parameter] public Icon? EndIcon { get; set; }

        [Parameter] public ICommand? Command { get; set; }

        [Parameter] public Appearance Appearance { get; set; } = Appearance.Hypertext;

        public override void SetFocuse()
        {
            
        }

        [Parameter] public string Href { get; set; } = "#";

    }
}
