using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components.Test;

public class JsRuntimeMock : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        // Intercept the dynamic import (common pattern: JSRuntime.InvokeAsync<IJSObjectReference>("import", path))
        if (identifier == "import" && typeof(TValue) == typeof(IJSObjectReference))
        {
            // return a dummy JS module reference
            var module = (TValue)(object)new DummyJsObjectReference();
            return ValueTask.FromResult(module);
        }

        // For other calls, return default(TValue)
        return ValueTask.FromResult(default(TValue)!);
    }

}

// Minimal IJSObjectReference implementation that accepts any Invoke / InvokeVoid calls and returns defaults.
class DummyJsObjectReference : IJSObjectReference
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        // Return default for any module method call (e.g. "init", "enableColumnResizing", "checkColumnOptionsPosition", "resizeColumn", "resetColumnWidths")
        return ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
