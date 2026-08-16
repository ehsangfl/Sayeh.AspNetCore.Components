using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Windows.Input;

namespace Sayeh.AspNetCore.Components.Test;

public class TestCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public int SubscriberCount => CanExecuteChanged?.GetInvocationList().Length ?? 0;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) { }
}

// Conditionally renders a SayehButton so toggling Show off removes it from the render tree,
// triggering Blazor's real disposal pipeline - exactly what happens to a row's button component
// when it scrolls out of a virtualized DataGrid's window.
public class LeakTestHost : ComponentBase
{
    [Parameter] public bool Show { get; set; }
    [Parameter] public ICommand? Cmd { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Show)
        {
            builder.OpenComponent<SayehButton>(0);
            builder.AddAttribute(1, nameof(SayehButton.Command), Cmd);
            builder.CloseComponent();
        }
    }
}

[TestClass]
public class SayehButtonCommandLeakTest : TestBase
{
    [TestMethod]
    public void DisposingButton_UnsubscribesFromCommand_CanExecuteChanged()
    {
        // Arrange: mirrors SayehCommandColumn binding the SAME shared ICommand instance to every
        // row's button (e.g. Central/Person's "Membership"/"Role" command columns) - as virtualized
        // scrolling creates and destroys row/button instances, each one that fails to unsubscribe
        // leaks a reference onto this single long-lived command.
        var command = new TestCommand();

        var cut = Render<LeakTestHost>(parameters => parameters
            .Add(p => p.Show, true)
            .Add(p => p.Cmd, command)
        );

        Assert.AreEqual(1, command.SubscriberCount,
            "Sanity check failed: rendering the button should subscribe exactly one handler to Command.CanExecuteChanged.");

        // Act: remove the button from the tree, triggering Blazor's renderer to dispose it -
        // exactly what happens every time a row scrolls out of the virtualized window.
        cut.Render(parameters => parameters.Add(p => p.Show, false));

        // Assert: the subscription must be removed, otherwise every scroll leaks another handler
        // referencing the (now disposed) button onto the shared command instance.
        Assert.AreEqual(0, command.SubscriberCount,
            $"Disposing the button should unsubscribe from Command.CanExecuteChanged, but {command.SubscriberCount} handler(s) remain - " +
            "this is a memory leak: every virtualized row that scrolls out of view leaks another subscription onto the shared command, " +
            "which matches production symptoms of scrolling getting progressively worse the longer you scroll.");
    }
}
