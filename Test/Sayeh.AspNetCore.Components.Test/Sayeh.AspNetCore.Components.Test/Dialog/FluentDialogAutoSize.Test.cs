using Microsoft.FluentUI.AspNetCore.Components;
using System.IO;
using System.Text.RegularExpressions;

namespace Sayeh.AspNetCore.Components.Test;

[TestClass]
public class FluentDialogAutoSizeTest : TestBase
{
    private const string AutoSizeClass = "sayeh-dialog-autosize";

    // FluentDialog.razor.cs: DEFAULT_DIALOG_WIDTH, always wrapped in calc() when no autosize override applies.
    private const string FluentDialogDefaultWidth = "500px";

    [TestMethod("FluentDialog Class parameter renders the marker class on the fluent-dialog host")]
    [Description("Declarative usage (<FluentDialog Class=\"sayeh-dialog-autosize\">) must put the marker " +
        "class directly on the <fluent-dialog> element, since reboot.css's fluent-dialog.sayeh-dialog-autosize " +
        "selector matches against that element.")]
    public void FluentDialog_ClassParameter_RendersOnHostElement()
    {
        var cut = Render<FluentDialog>(parameters => parameters
            .Add(p => p.Class, AutoSizeClass)
            .AddChildContent("content"));

        var dialog = cut.Find("fluent-dialog");
        StringAssert.Contains(dialog.ClassList.ToString(), AutoSizeClass);
    }

    [TestMethod("FluentDialogBody Class parameter renders the marker class on its own element")]
    [Description("DialogService-based usage has no way to set Class on the fluent-dialog host itself, so " +
        "the opt-in class is placed on inner content instead (e.g. <FluentDialogBody Class=\"sayeh-dialog-autosize\">) " +
        "and matched via reboot.css's fluent-dialog:has(.sayeh-dialog-autosize) selector.")]
    public void FluentDialogBody_ClassParameter_RendersOnBodyElement()
    {
        var cut = Render<FluentDialogBody>(parameters => parameters
            .Add(p => p.Class, AutoSizeClass)
            .AddChildContent("content"));

        var body = cut.Find(".fluent-dialog-body");
        StringAssert.Contains(body.ClassList.ToString(), AutoSizeClass);
    }

    [TestMethod("reboot.css autosize rule declares a content-driven width, not FluentDialog's fixed default")]
    [Description("bUnit has no CSS/layout engine, so pixel width can't be measured directly. This instead " +
        "verifies the actual override rule in reboot.css sets a fit-content width that differs from " +
        "FluentDialog's built-in fixed default, proving the override isn't a no-op.")]
    public void AutoSizeCssRule_WidthDiffersFromFluentDialogDefault()
    {
        var css = ReadRebootCss();

        var rule = Regex.Match(css, @"fluent-dialog\.sayeh-dialog-autosize[^{]*\{([^}]*)\}");
        Assert.IsTrue(rule.Success, "Could not find the fluent-dialog.sayeh-dialog-autosize rule in reboot.css");

        var declaredWidth = Regex.Match(rule.Groups[1].Value, @"width:\s*([^;]+);").Groups[1].Value.Trim();

        Assert.AreNotEqual(FluentDialogDefaultWidth, declaredWidth);
        Assert.AreEqual("fit-content !important", declaredWidth);
    }

    private static string ReadRebootCss([CallerFilePath] string testFilePath = "")
    {
        var testDir = Path.GetDirectoryName(testFilePath)!;
        var cssPath = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", "..",
            "src", "Sayeh.AspNetCore.Components", "wwwroot", "reboot.css"));
        return File.ReadAllText(cssPath);
    }
}
