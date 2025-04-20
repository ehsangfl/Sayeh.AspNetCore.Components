using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Blazored.LocalStorage;
using System.Globalization;
using Sayeh.AspNetCore.Essentials.WebAssembly;
using Sayeh.AspNetCore.Essentials;
using Microsoft.Extensions.Hosting;

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
