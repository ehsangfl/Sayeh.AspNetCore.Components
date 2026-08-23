namespace Sayeh.AspNetCore.Components.Test;

public class ActionColumnTestRow
{
    public int Id { get; set; }
}

[TestClass]
public class SayehActionColumnTest : TestBase
{
    [TestMethod]
    public async Task RevealOnHover_Default_RendersHiddenActionsCell()
    {
        var items = new List<ActionColumnTestRow> { new() { Id = 1 } };

        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehActionColumn<ActionColumnTestRow>));
            builder.AddAttribute(seq++, nameof(SayehActionColumn<ActionColumnTestRow>.ChildContent),
                (RenderFragment<ActionColumnTestRow>)(item => b => b.AddContent(0, "edit")));
            builder.CloseComponent();
        };

        var cut = Render<SayehDataGrid<ActionColumnTestRow>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent(gridChild)
        );
        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());

        var cell = cut.Find(".sayeh-action-cell");
        Assert.IsFalse(cell.ClassList.Contains("sayeh-action-cell-static"),
            "By default the actions cell should rely on hover/focus reveal rather than always being visible.");
        Assert.IsTrue(cell.TextContent.Contains("edit"), "The action content should still be rendered into the DOM.");
    }

    [TestMethod]
    public async Task RevealOnHoverFalse_RendersAlwaysVisibleActionsCell()
    {
        var items = new List<ActionColumnTestRow> { new() { Id = 1 } };

        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehActionColumn<ActionColumnTestRow>));
            builder.AddAttribute(seq++, nameof(SayehActionColumn<ActionColumnTestRow>.RevealOnHover), false);
            builder.AddAttribute(seq++, nameof(SayehActionColumn<ActionColumnTestRow>.ChildContent),
                (RenderFragment<ActionColumnTestRow>)(item => b => b.AddContent(0, "edit")));
            builder.CloseComponent();
        };

        var cut = Render<SayehDataGrid<ActionColumnTestRow>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent(gridChild)
        );
        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());

        var cell = cut.Find(".sayeh-action-cell");
        Assert.IsTrue(cell.ClassList.Contains("sayeh-action-cell-static"),
            "RevealOnHover=false should keep the actions cell always visible.");
    }
}
