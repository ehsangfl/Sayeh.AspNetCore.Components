using Sayeh.Essentials.Core;

namespace Sayeh.AspNetCore.Essentials.Server;

public class LocalizationManager() : ILocalizationManager
{
    public Task ChangeCulture(string NewCulture)
    {
        System.Globalization.CultureInfo.CurrentUICulture
            = System.Globalization.CultureInfo.CurrentCulture
            = new System.Globalization.CultureInfo(NewCulture);
        return Task.CompletedTask;
    }

    public ValueTask<string> GetCurrentCulture()
    {
        return ValueTask.FromResult(System.Globalization.CultureInfo.CurrentUICulture.Name);
    }

    public void SetCulture(string Culture)
    {
        if (string.IsNullOrEmpty(Culture))
            Culture = "fa-IR";
        if (System.Globalization.CultureInfo.CurrentUICulture is null || !Culture.Equals(System.Globalization.CultureInfo.CurrentUICulture.Name))
        {
            var NewCulture = new System.Globalization.CultureInfo(Culture);
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture
                = System.Globalization.CultureInfo.DefaultThreadCurrentUICulture
                = System.Globalization.CultureInfo.CurrentUICulture
                = System.Globalization.CultureInfo.CurrentCulture = NewCulture;

        }
    }
}
