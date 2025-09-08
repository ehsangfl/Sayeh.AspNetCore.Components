using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Input;

namespace Sayeh.Essentials.Core.MVVM;

public class RelayCommand<T> : ICommand
{
    private readonly WeakAction<T?> _execute;

    private readonly WeakFunc<T?, bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the RelayCommand class that 
    /// can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
    public RelayCommand(Action<T?> execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the RelayCommand class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");

        _execute = new WeakAction<T?>(execute);

        if (canExecute != null)
            _canExecute = new WeakFunc<T?, bool>(canExecute);
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged" /> event.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "The this keyword is used in the Silverlight version")]
    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
    public void RaiseCanExecuteChanged()
    {
        var handler = CanExecuteChanged;
        if (handler != null)
            handler(this, EventArgs.Empty);
    }

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data 
    /// to be passed, this object can be set to a null reference</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null)
            return true;

        if (_canExecute.IsStatic || _canExecute.IsAlive)
        {
            if (parameter == null && typeof(T).GetTypeInfo().IsValueType)
                return _canExecute.Execute(default(T));

            if (parameter == null || parameter is T)
                return (_canExecute.Execute((T?)parameter));
        }

        return false;
    }

    /// <summary>
    /// Defines the method to be called when the command is invoked. 
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data 
    /// to be passed, this object can be set to a null reference</param>
    public virtual void Execute(object? parameter)
    {
        var val = parameter;

        #region this part can be removed. i think this is for dotnetframework

        if (parameter != null && parameter.GetType() != typeof(T))
        {
            if (parameter is IConvertible)
                val = Convert.ChangeType(parameter, typeof(T), null);
        }

        #endregion

        if (CanExecute(val) && _execute != null && (_execute.IsStatic || _execute.IsAlive))
        {
            if (val == null)
            {
                if (typeof(T).GetTypeInfo().IsValueType)
                    _execute.Execute(default(T));
                else
                    _execute.Execute((T?)val);
            }
            else
                _execute.Execute((T)val);
        }
    }
}
