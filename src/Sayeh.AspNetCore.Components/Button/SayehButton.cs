using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sayeh.AspNetCore.Components
{

    /// <summary>
    /// this is MVVM freindly button with command and commandparameter properties
    /// </summary>
    public class SayehButton : FluentButton, IAsyncDisposable
    {
        #region Properties

        private ICommand? _command;
        [Parameter] public ICommand? Command { get; set; }

        private object? _commandParameter;
        [Parameter] public object? CommandParameter { get; set; }

        #endregion

        #region Initialization

        public SayehButton()
        {
            this.OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, OnClick_Override);
        }

        #endregion

        #region Functions

        protected override void OnParametersSet()
        {
            if (Command != _command)
            {
                {
                    if (_command != null)
                        _command.CanExecuteChanged -= OnCommandCanExecuteChanged;
                    _command = Command;
                    if (_command != null)
                    {
                        _command.CanExecuteChanged += OnCommandCanExecuteChanged;
                        Disabled = !_command.CanExecute(CommandParameter);
                    }
                }
            }
            if (_commandParameter != CommandParameter)
            {
                _commandParameter = CommandParameter;
                if (Command is not null)
                    Disabled = !Command.CanExecute(_commandParameter);
            }
            base.OnParametersSet();
        }

        private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
        {
            Disabled = !Command!.CanExecute(CommandParameter);
            StateHasChanged();
        }

        private void OnClick_Override(MouseEventArgs e)
        {
            if (Command != null)
                Command.Execute(CommandParameter);
        }

        // FluentButton.DisposeAsync() isn't virtual, so this re-declares IAsyncDisposable to hook
        // into the renderer's disposal path (it checks IAsyncDisposable first). Without this,
        // rows created and torn down repeatedly during virtualized scrolling (e.g. SayehCommandColumn
        // buttons bound to a shared ICommand) would leak a CanExecuteChanged subscription per row.
        public new async ValueTask DisposeAsync()
        {
            if (_command is not null)
                _command.CanExecuteChanged -= OnCommandCanExecuteChanged;
            await base.DisposeAsync();
        }

        #endregion

    }
}
