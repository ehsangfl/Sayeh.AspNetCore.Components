using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sayeh.AspNetCore.Components
{
    public class ObservableCollectionComponent<T> : ComponentBase , IDisposable where T :class, IEnumerable, INotifyCollectionChanged
    {

        T? _itemsSource;
        [Parameter] 
        public T? ItemsSource { get; set; }

        [Parameter] 
        public RenderFragment? ChildContent { get; set; }

        protected override void OnInitialized()
        {
            if (ItemsSource is not null)
            {
                _itemsSource = ItemsSource;
                ItemsSource.As<INotifyCollectionChanged>().CollectionChanged += OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }

        protected override void OnParametersSet()
        {
            if (_itemsSource != ItemsSource)
            {
                if (_itemsSource is not null)
                    _itemsSource.CollectionChanged -= OnCollectionChanged;

                if (ItemsSource is not null)
                    ItemsSource.CollectionChanged += OnCollectionChanged;

                _itemsSource = ItemsSource;
            }
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
            if (disposing && ItemsSource is not null)
              ItemsSource.As<INotifyCollectionChanged>().CollectionChanged += OnCollectionChanged;
        }

        public void Dispose()
        {
           Dispose(true);
        }
    }
}
