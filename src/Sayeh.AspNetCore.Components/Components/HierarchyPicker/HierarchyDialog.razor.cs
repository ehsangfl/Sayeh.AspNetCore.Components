using Microsoft.FluentUI.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components;

partial class HierarchyDialog<TItem> where TItem : class
{

    internal record Parameters(IEnumerable<TItem>? Items, Func<TItem, TItem?>? ParentItem, Func<TItem, IEnumerable<TItem>>? Children, RenderFragment<TItem>? ItemTemplate, Func<TItem, string>? DisplayText, bool Virtualize);

    #region Properties

    [Parameter]
    public TItem? Content { get; set; }

    public IEnumerable<TItem>? Items { get; set; }

    public Func<TItem, IEnumerable<TItem>>? Children { get; set; }

    public RenderFragment<TItem>? ItemTemplate { get; set; }

    public bool Virtualize { get; set; }

    public Func<TItem, string>? DisplayText { get; set; }

    public Func<TItem, TItem?>? ParentItem { get; set; }

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    #endregion

    protected override void OnInitialized()
    {
        SetParameters();
        base.OnInitialized();
    }

    #region Functions

    void SetParameters()
    {
        var parameters = Dialog.Instance.Parameters["Parameters"] as Parameters;
        if (parameters is null)
            return;
        Items = parameters.Items;
        Children = parameters.Children;
        ItemTemplate = parameters.ItemTemplate;
        Virtualize = parameters.Virtualize;
        DisplayText = parameters.DisplayText;
        ParentItem = parameters.ParentItem;
    }

    async void CloseModal()
    {
        await Dialog.CloseAsync(DialogResult.Ok(Content));
    }

    #endregion


}
