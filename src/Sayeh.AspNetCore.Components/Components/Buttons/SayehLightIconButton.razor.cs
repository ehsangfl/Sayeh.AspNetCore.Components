using System.Windows.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components
{
    // Plain span rather than SayehButton/FluentButton: this is meant to be dropped into places that
    // render one instance per row (e.g. SayehActionColumn), where hundreds of rows can mount and
    // unmount per scroll session. FluentButton's Shadow-DOM web component is expensive to
    // instantiate/destroy at that churn rate; a plain span carries none of that cost. CanExecute is
    // re-evaluated on every render instead of cached via a CanExecuteChanged subscription, so there's
    // no per-instance event subscription to leak.
    partial class SayehLightIconButton<TItem>
    {
        [Parameter, EditorRequired] public Icon Icon { get; set; } = default!;

        [Parameter, EditorRequired] public ICommand Command { get; set; } = default!;

        [Parameter, EditorRequired] public TItem Item { get; set; } = default!;

        [Parameter] public string? Title { get; set; }

        private bool CanExecute => Command?.CanExecute(Item) ?? true;

        private void OnClickHandler()
        {
            if (CanExecute)
                Command?.Execute(Item);
        }

        private void OnKeyDownHandler(KeyboardEventArgs e)
        {
            if (CanExecute && (e.Key == "Enter" || e.Key == " "))
                Command?.Execute(Item);
        }
    }
}
