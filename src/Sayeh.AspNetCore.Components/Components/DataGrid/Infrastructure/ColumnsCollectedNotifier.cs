using Microsoft.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components.DataGrid.Infrastructure
{
    public sealed class ColumnsCollectedNotifier<TItem> : IComponent where TItem : class
    {

        private bool _isFirstRender = true;

        [CascadingParameter] internal InternalGridContext<TItem> InternalGridContext { get; set; } = default!;

        /// <inheritdoc/>
        public void Attach(RenderHandle renderHandle)
        {
            // This component never renders, so we can ignore the renderHandle
        }

        /// <inheritdoc/>
        public Task SetParametersAsync(ParameterView parameters)
        {
            if (_isFirstRender)
            {
                _isFirstRender = false;
                parameters.SetParameterProperties(this);
                return InternalGridContext.ColumnsFirstCollected.InvokeCallbacksAsync(null);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

    }
}
