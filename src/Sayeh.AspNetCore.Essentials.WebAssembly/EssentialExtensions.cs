using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using Sayeh.Essentials.Core;

namespace Sayeh.AspNetCore.Essentials.WebAssembly
{
    public static class EssentialExtensions
    {

        public static IServiceCollection AddSayehEssentials(this IServiceCollection services)
        {
           services.AddTransient<ILocalizationManager,LocalizationManager>();
           services.AddBlazoredLocalStorage();
           return services.AddTransient<ILocalStorage, LocalStorage>();
        }

    }
}
