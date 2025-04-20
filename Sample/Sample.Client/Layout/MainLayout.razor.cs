using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Globalization;

namespace Sample.Client.Layout
{
    partial class MainLayout
    {
        [Inject]
        public ILocalStorageService LocalStorage { get; set; } = default!;

        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Inject]
        public GlobalState? GlobalState { get; set; }

        LocalizationDirection Direction { get; set; }

        private string? selectedCulture;

        protected override void OnInitialized()
        {
            selectedCulture = CultureInfo.CurrentCulture.Name;
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                Direction = (System.Globalization.CultureInfo.DefaultThreadCurrentCulture?.TextInfo.IsRightToLeft ?? false) 
                                        ? LocalizationDirection.RightToLeft : LocalizationDirection.LeftToRight;
                GlobalState?.SetDirection(Direction);
                StateHasChanged();
            }

        }

        private async void changeCulture(string culture)
        {
            if (culture  is null || new CultureInfo(culture)!.Name == CultureInfo.CurrentUICulture.Name)
                return;

            var newCulture = new System.Globalization.CultureInfo(culture);
            await LocalStorage.SetItemAsync("culture", culture);
            var uri = new Uri(Navigation.Uri)
                .GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);

            var cultureEscaped = Uri.EscapeDataString(culture);
            var uriEscaped = Uri.EscapeDataString(uri);

            Navigation.NavigateTo(
                $"Culture/Set?culture={cultureEscaped}&redirectUri={uriEscaped}",
                forceLoad: true);
        }
    }
}
