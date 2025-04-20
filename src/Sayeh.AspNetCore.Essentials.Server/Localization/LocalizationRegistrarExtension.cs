using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sayeh.Essentials.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Essentials.Server
{
    internal static class LocalizationRegistrarExtension
    {
        public static IServiceCollection AddLocalizationSupport(this IServiceCollection services, SayehOptions options)
        {
            services.AddTransient<ILocalizationManager, LocalizationManager>();
            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(opt =>
            {
                opt.AddSupportedCultures(options.Localization.SupportedCultures).AddSupportedUICultures(options.Localization.SupportedCultures).SetDefaultCulture(options.Localization.SupportedCultures[0]);
                opt.RequestCultureProviders.Remove(opt.RequestCultureProviders.First(w => w is Microsoft.AspNetCore.Localization.AcceptLanguageHeaderRequestCultureProvider));
                opt.RequestCultureProviders.Insert(0, new IdentityRequestCultureProvider(options));
                opt.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider() { BindDefaultCulture = options.Localization.BindDefaultCulture });
            });


            return services;
        }

    }
}
