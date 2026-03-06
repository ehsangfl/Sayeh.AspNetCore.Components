
using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.Essentials.Core;

namespace Sayeh.AspNetCore.Components;

partial class SayehHierarchyPicker<TItem> where TItem : class
{

    #region Fields

    FluentAutocomplete<TItem> _autocomplete = default!;

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
    public bool Virtualize { get; set; }

    [Parameter]
    public Func<TItem, TItem?>? ParentItem { get; set; }

    [Inject]
    public IDialogService DialogService { get; set; } = default!;

    [Parameter]
    public EventCallback<OptionsSearchEventArgs<TItem>> Filter { get; set; }

    //
    // Summary:
    //     Gets or sets the function used to determine which text to display for each option.
    [Parameter]
    public virtual Func<TItem, string?> OptionText { get; set; } = default!;

    //
    // Summary:
    //     Gets or sets the function used to determine which value to return for the selected
    //     item. Only for Microsoft.FluentUI.AspNetCore.Components.FluentListbox`1 and Microsoft.FluentUI.AspNetCore.Components.FluentSelect`1
    //     components.
    [Parameter]
    [EditorRequired]
    public virtual Func<TItem, string?>? OptionValue { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = default!;

    #endregion

    #region Initialize

    #endregion

    #region Functions

    async void OpenDialog() {
        await _autocomplete.CloseDropdownAsync();
        var parameter = new DialogParameters<TItem>() { Modal = true, Content = SelectedItem  };
        
        parameter["Parameters"] = new HierarchyDialog<TItem>.Parameters(
            Items?.Where(w => ParentItem?.Invoke(w) is null),
            ParentItem,
            Children,
            TreeItemTemplate.Or(ItemTemplate),
            OptionText,
            Virtualize);
        
        //DialogService.CreateDialogCallback(this, TreeCallback);
        var reference = await DialogService.ShowPanelAsync<TItem>(typeof(HierarchyDialog<TItem>), SelectedItem!,parameter);
        var result = await reference.Result;
        await TreeCallback(result);
    }

    void OnOptionsSearch(OptionsSearchEventArgs<TItem> e)
    {
        if (Filter.HasDelegate)
            Filter.InvokeAsync(e);
        else if (OptionText is not null) {
            var txt = e.Text.Remove(" ");
            e.Items = Items?.Where(w => OptionText.Invoke(w)?.Remove(" ")?.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ?? false);
        }
        else
        {
            var txt = e.Text.Remove(" ");
            e.Items = Items?.Where(w => w?.ToString()?.Remove(" ")?.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ?? false);
        }
            
    }

    private Task TreeCallback(DialogResult result)
    {
        if (result.Cancelled)
            return Task.CompletedTask;
        SelectedItem = result.Data as TItem;
        InvokeAsync(StateHasChanged);
        return Task.CompletedTask;
    }


    #endregion

}
