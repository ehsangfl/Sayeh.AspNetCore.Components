using Sample.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Sayeh.AspNetCore.Essentials.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddFluentUIComponents();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLocalization();

builder.Services.AddSayehEssentials(opt => {
    opt.Localization.SupportedCultures = new[] { "fa-IR", "en-US" };
    opt.Localization.BindDefaultCulture = true;
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStaticFiles();
app.UseAntiforgery();

app.UseRequestLocalization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Sample.Client._Imports).Assembly);

app.Run();
