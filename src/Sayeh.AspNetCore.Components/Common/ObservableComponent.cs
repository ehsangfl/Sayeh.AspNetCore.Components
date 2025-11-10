using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.ComponentModel;

namespace Sayeh.AspNetCore.Components;

public class ObservableComponent<T> : ComponentBase, IDisposable
    where T : class, INotifyPropertyChanged
{
    private T? _itemSource;
    [Parameter]
    public T? ItemSource { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        if (ItemSource is not null)
        {
            _itemSource = ItemSource;
            ItemSource.PropertyChanged += OnModelPropertyChanged;
        }
    }

    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(_itemSource, ItemSource))
        {
            if (_itemSource is not null)
                _itemSource.PropertyChanged -= OnModelPropertyChanged;

            if (ItemSource is not null)
                ItemSource.PropertyChanged += OnModelPropertyChanged;

            _itemSource = ItemSource;
            Console.WriteLine($"itemSource change detected for and its value is {ItemSource?.ToString()}");
            InvokeAsync(StateHasChanged);
        }
    }


    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Console.WriteLine($"{e.PropertyName?.ToString()} change detected for {ItemSource?.GetType().FullName}");
        InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is not null)
        {
            builder.AddContent(0, ChildContent);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && ItemSource is not null)
            ItemSource.PropertyChanged -= OnModelPropertyChanged;
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
