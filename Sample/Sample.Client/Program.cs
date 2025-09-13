global using Sayeh.Essentials.Core;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.FluentUI.AspNetCore.Components;
global using Blazored.LocalStorage;
global using System.Globalization;
global using Sayeh.AspNetCore.Essentials.WebAssembly;
global using Sayeh.AspNetCore.Essentials;
global using Microsoft.Extensions.Hosting;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddFluentUIComponents();
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddLocalization();
builder.Services.AddSayehEssentials();

var app = builder.Build();

var localizeManager = app.Services.GetService<LocalizationManager>();
if (localizeManager is not null)
{
    localizeManager.SetCulture(await localizeManager.GetCurrentCulture());
}

await app.RunAsync();
