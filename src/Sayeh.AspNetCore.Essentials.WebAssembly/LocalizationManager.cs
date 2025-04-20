using Sayeh.Essentials.Core;

namespace Sayeh.AspNetCore.Essentials.WebAssembly;

public class LocalizationManager(ILocalStorage localStorage) : ILocalizationManager
{
    public async Task ChangeCulture(string NewCulture)
    {
        if (string.IsNullOrEmpty(NewCulture))
            NewCulture = "fa-IR";
        if (System.Globalization.CultureInfo.CurrentUICulture == null || !NewCulture.Equals(System.Globalization.CultureInfo.CurrentUICulture.Name))
        {
            await localStorage.WriteItem("culture", NewCulture);
            SetCulture(NewCulture);
        }
    }

    public async ValueTask<string> GetCurrentCulture()
    {
        string? Culture = string.Empty;
        if (string.IsNullOrEmpty(Culture))
            Culture = await localStorage.ReadItem<string>("culture");
        if (string.IsNullOrEmpty(Culture))
            Culture = "fa-IR";
        return Culture;
    }

    public void SetCulture(string Culture)
    {
        if (string.IsNullOrEmpty(Culture))
            Culture = "fa-IR";
        if (System.Globalization.CultureInfo.CurrentUICulture is null || !Culture.Equals(System.Globalization.CultureInfo.CurrentUICulture.Name))
        {
            var NewCulture = new System.Globalization.CultureInfo(Culture);
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = NewCulture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = NewCulture;
            System.Globalization.CultureInfo.CurrentUICulture = NewCulture;
            System.Globalization.CultureInfo.CurrentCulture = NewCulture;
        }
    }
}
