using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components.Test;

public class TestBase : BunitContext
{
    [TestInitialize]
    public void InitializeTest() 
    {
        Services.AddFluentUIComponents();
        Services.AddSingleton<IJSRuntime>(new JsRuntimeMock());
    }
}
