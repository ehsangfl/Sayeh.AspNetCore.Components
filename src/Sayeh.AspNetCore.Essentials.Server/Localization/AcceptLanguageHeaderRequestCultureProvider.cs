using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Net.Http.Headers;

namespace Sayeh.AspNetCore.Essentials.Server
{
    internal class AcceptLanguageHeaderRequestCultureProvider: RequestCultureProvider
    {
        public bool BindDefaultCulture { get; set; }

        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var LocalizerOption = httpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>)) as Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>;

            if (BindDefaultCulture && LocalizerOption is not null)
                return Task.FromResult(new ProviderCultureResult(LocalizerOption.Value.DefaultRequestCulture.UICulture.Name))!;

            var acceptLanguageHeader = httpContext.Request.GetTypedHeaders().AcceptLanguage;

            if (acceptLanguageHeader == null || acceptLanguageHeader.Count == 0)
            {
                return NullProviderCultureResult;
            }

            var languages = acceptLanguageHeader.AsEnumerable();

            var orderedLanguages = languages.OrderByDescending(h => h, StringWithQualityHeaderValueComparer.QualityComparer).Select(x => x.Value).ToList();

            if (orderedLanguages.Count > 0)
            {
                if (!LocalizerOption?.Value.DefaultRequestCulture.None() ?? false)
                {
                    var Default = LocalizerOption!.Value.DefaultRequestCulture.UICulture.Name;
                    if (orderedLanguages.Contains(Default))
                    {
                        orderedLanguages.Remove(Default);
                        orderedLanguages.Insert(0, Default);
                    }
                    else
                        return NullProviderCultureResult;
                }
                return Task.FromResult(new ProviderCultureResult(orderedLanguages))!;
            }

            return NullProviderCultureResult;
        }
    }
}
