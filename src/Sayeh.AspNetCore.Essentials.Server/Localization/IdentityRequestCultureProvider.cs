using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Essentials.Server
{
    internal class IdentityRequestCultureProvider : RequestCultureProvider
    {

        private readonly SayehOptions _options;

        public IdentityRequestCultureProvider(SayehOptions options)
        {
            _options = options;
        }

        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            if ((!httpContext.User?.Identity?.IsAuthenticated) ?? true)
                return NullProviderCultureResult;
            var cultureClaim = httpContext.User?.Claims.FirstOrDefault(f => f.Type == _options.Localization.CultureClaimType);
            if (cultureClaim is null || cultureClaim.Value.None())
                return NullProviderCultureResult;
            else
                return Task.FromResult(new ProviderCultureResult(cultureClaim.Value))!;

        }
    }
}
