using Microsoft.FluentUI.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components;

partial class HierarchyDialog<TItem> where TItem : class
{

    internal record Parameters(IEnumerable<TItem>? Items
        , Func<TItem, TItem?>? ParentItem
        , Func<TItem, IEnumerable<TItem>>? Children
        , RenderFragment<TItem>? ItemTemplate
        , RenderFragment? TreeChildContent
        , Func<TItem, string?>? DisplayMember
        , bool Virtualize);

    #region Properties

    [Parameter]
    public TItem? Content { get; set; }

    public IEnumerable<TItem>? Items { get; set; }

    public Func<TItem, IEnumerable<TItem>>? Children { get; set; }

    public RenderFragment<TItem>? ItemTemplate { get; set; }

    public bool Virtualize { get; set; }

    public Func<TItem, string>? DisplayMember { get; set; }

    public Func<TItem, string>? ValueMember { get; set; }

    public Func<TItem, TItem?>? ParentItem { get; set; }

    public RenderFragment? ChildContent { get; set; }

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    /// <summary>
    /// Tree selection, decoupled from the <see cref="Content"/> parameter. FluentDialog re-passes
    /// Content on every one of its own re-renders (FluentDialog.razor's DynamicComponent gets
    /// Parameters from DialogInstance.GetParameterDictionary(), which always holds the original
    /// object the dialog was opened with - see DialogInstance.Content). Binding the tree directly
    /// to Content would get silently reset back to that original value by the next such
    /// re-render (busy indicator, focus trap JS interop, etc.), discarding the user's pick before
    /// "Select" is even clicked. Seeded once from Content in OnInitialized; never written back to
    /// the Content parameter itself.
    /// </summary>
    private TItem? _selectedItem;

    #endregion

    protected override void OnInitialized()
    {
        SetParameters();
        _selectedItem = Content;
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
        DisplayMember = parameters.DisplayMember;
        ParentItem = parameters.ParentItem;
        ChildContent = parameters.TreeChildContent;
    }

    async void CloseModal()
    {
        await Dialog.CloseAsync(DialogResult.Ok(_selectedItem));
    }

    #endregion


}
