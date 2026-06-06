
using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.Essentials.Core;
using System.ComponentModel;
using System.Linq;

namespace Sayeh.AspNetCore.Components;

partial class SayehHierarchyPicker<TItem> where TItem : class
{

    #region Fields

    FluentAutocomplete<TItem> _autocomplete = default!;
    private TItem? _internalSelectedItem;

    #endregion

    #region Properties

    [Parameter]
    public Func<TItem, IEnumerable<TItem>>? Children { get; set; }

    [Parameter]
    public TItem? SelectedItem { get; set; }

    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    [Parameter]
    public RenderFragment<TItem>? TreeItemTemplate { get; set; }

    [Parameter]
    public RenderFragment? TreeChildContent { get; set; }

    [Parameter]
    public bool Virtualize { get; set; }

    [Parameter]
    public Func<TItem, TItem?>? Parent { get; set; }

    [Inject]
    public IDialogService DialogService { get; set; } = default!;

    [Parameter]
    public EventCallback<OptionsSearchEventArgs<TItem>> Filter { get; set; }

    //
    // Summary:
    //     Gets or sets the function used to determine which text to display for each option.
    [Parameter]
    public virtual Func<TItem, string?> DisplayMember { get; set; } = default!;

    //
    // Summary:
    //     Gets or sets the function used to determine which value to return for the selected
    //     item. Only for Microsoft.FluentUI.AspNetCore.Components.FluentListbox`1 and Microsoft.FluentUI.AspNetCore.Components.FluentSelect`1
    //     components.
    [Parameter]
    [EditorRequired]
    public virtual Func<TItem, string?>? ValueMember { get; set; } = default!;

    /// <summary>
    /// Set flatten items, the TreeView dialog uses TreeItems property or if its null, find nodes by parent and child properties
    /// </summary>
    [Parameter]
    [EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = default!;

    /// <summary>
    /// Set Hierarchical items, if is null, tree items calculated based on Parent/Children property
    /// </summary>
    [Parameter]
    public IEnumerable<TItem> TreeItems { get; set; } = default!;

    [Parameter]
    public string? Label { get; set; }

    #endregion

    #region Events

    [Parameter]
    public EventCallback<TItem?> SelectedItemChanged { get; set; }

    #endregion

    #region Initialize
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _internalSelectedItem = SelectedItem;
    }

    #endregion

    #region Functions

    async void OpenDialog()
    {
        await _autocomplete.CloseDropdownAsync();
        var parameter = new DialogParameters<TItem>() { Modal = true, Content = SelectedItem };

        parameter["Parameters"] = new HierarchyDialog<TItem>.Parameters(
            TreeItems ?? (Parent is null ? Items : Items?.Where(w => Parent?.Invoke(w) is null)),
            Parent,
            Children,
            TreeItemTemplate.Or(ItemTemplate),
            TreeChildContent,
            DisplayMember,
            Virtualize);

        //DialogService.CreateDialogCallback(this, TreeCallback);
        var reference = await DialogService.ShowPanelAsync<TItem>(typeof(HierarchyDialog<TItem>), SelectedItem!, parameter);
        var result = await reference.Result;
        await TreeCallback(result);
    }

    void OnOptionsSearch(OptionsSearchEventArgs<TItem> e)
    {
        if (Filter.HasDelegate)
            Filter.InvokeAsync(e);
        else if (DisplayMember is not null)
        {
            var txt = e.Text.Remove(" ");
            e.Items = Items?.Where(w => DisplayMember.Invoke(w)?.Remove(" ")?.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ?? false);
        }
        else
        {
            var txt = e.Text.Remove(" ");
            e.Items = Items?.Where(w => w?.ToString()?.Remove(" ")?.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ?? false);
        }
        var items = e.Items;
        if (SelectedItem is not null && items is not null)
        {
            items = new List<TItem>() { SelectedItem }.Union(items);
        }
        e.Items = items;
    }

    private async Task TreeCallback(DialogResult result)
    {
        if (result.Cancelled)
            return;
        await UpdateSelectedItemAsync(result.Data as TItem);
    }

    // Called by the Autocomplete set binding
    private Task SetSelectedItem(TItem? item) =>
        UpdateSelectedItemAsync(item);

    // Centralized updater: notify parent and update component UI
    private async Task UpdateSelectedItemAsync(TItem? item)
    {
        if (!EqualityComparer<TItem?>.Default.Equals(_internalSelectedItem, item))
        {
            _internalSelectedItem = item;
            if (SelectedItemChanged.HasDelegate)
            {
                await SelectedItemChanged.InvokeAsync(item);
            }
            await InvokeAsync(StateHasChanged);
        }
    }

    #endregion

}
