using Microsoft.Extensions.DependencyInjection;
using Sayeh.Essentials.Core;

namespace Sayeh.AspNetCore.Essentials.Server;

public static class EssentialExtensions
{

    public static IServiceCollection AddSayehEssentials(this IServiceCollection services, Action<SayehOptions>? configuration = null)
    {
        var opt = new SayehOptions();
        if (configuration is not null) 
            configuration.Invoke(opt);
        services.AddLocalizationSupport(opt);
        return services;
    }

}


