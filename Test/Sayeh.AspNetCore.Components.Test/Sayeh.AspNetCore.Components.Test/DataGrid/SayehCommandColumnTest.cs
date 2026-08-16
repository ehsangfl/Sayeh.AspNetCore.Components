using System.Linq.Expressions;
using System.Windows.Input;

namespace Sayeh.AspNetCore.Components.Test;

public class CommandColumnTestRow
{
    public int Id { get; set; }
}

public class RecordingCommand : ICommand
{
    public object? LastExecutedParameter { get; private set; }
    public int ExecuteCount { get; private set; }
    public bool AllowExecute { get; set; } = true;

    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => AllowExecute;
    public void Execute(object? parameter)
    {
        ExecuteCount++;
        LastExecutedParameter = parameter;
    }
}

[TestClass]
public class SayehCommandColumnTest : TestBase
{
    [TestMethod]
    public async Task Click_ExecutesCommand_WithRowAsParameter()
    {
        var items = new List<CommandColumnTestRow> { new() { Id = 1 }, new() { Id = 2 } };
        var command = new RecordingCommand();

        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehCommandColumn<CommandColumnTestRow>));
            builder.AddAttribute(seq++, nameof(SayehCommandColumn<CommandColumnTestRow>.Command), command);
            builder.CloseComponent();
        };

        var cut = Render<SayehDataGrid<CommandColumnTestRow>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent(gridChild)
        );
        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());

        // The command column must NOT render a heavy FluentButton/SayehButton (Shadow-DOM web
        // component) - that's the whole point of this change, since virtualized rows churn through
        // hundreds of these during a scroll session.
        Assert.IsFalse(cut.Markup.Contains("fluent-button"),
            "SayehCommandColumn should render a plain element, not a FluentButton, to stay cheap under virtualized row churn. Markup: " + cut.Markup);

        var link = cut.Find(".sayeh-command-link");
        await cut.InvokeAsync(() => link.Click());

        Assert.AreEqual(1, command.ExecuteCount, "Clicking the command link should execute the command exactly once.");
        Assert.AreEqual(items[0], command.LastExecutedParameter, "The command should execute with the row item as its parameter.");
    }

    [TestMethod]
    public async Task Click_WhenCanExecuteIsFalse_DoesNotExecuteCommand()
    {
        var items = new List<CommandColumnTestRow> { new() { Id = 1 } };
        var command = new RecordingCommand { AllowExecute = false };

        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehCommandColumn<CommandColumnTestRow>));
            builder.AddAttribute(seq++, nameof(SayehCommandColumn<CommandColumnTestRow>.Command), command);
            builder.CloseComponent();
        };

        var cut = Render<SayehDataGrid<CommandColumnTestRow>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent(gridChild)
        );
        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());

        var link = cut.Find(".sayeh-command-link");
        Assert.IsTrue(link.ClassList.Contains("sayeh-command-link-disabled"),
            "The link should carry the disabled class when Command.CanExecute returns false.");

        await cut.InvokeAsync(() => link.Click());

        Assert.AreEqual(0, command.ExecuteCount, "Clicking a disabled command link must not execute the command.");
    }
}
