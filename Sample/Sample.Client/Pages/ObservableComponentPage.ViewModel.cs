using Sample.Client.Model;
using System.ComponentModel;

namespace Sample.Client.Pages
{
    public class ObservableComponentPage_ViewModel : INotifyPropertyChanged
    {

        public async Task LoadData() {
            await Task.Delay(2000);
            Model = new WeatherForecastModel_NotifyPropertyChanged() { Order = 1, Date = DateTime.Now, DayofWeek = "the today", Tempreture = -5, Type = DayType.Snowy, Description = "no reason" };
        }

        private WeatherForecastModel_NotifyPropertyChanged _model;
        public WeatherForecastModel_NotifyPropertyChanged Model
        {
            get { return _model; }
            private set
            {
                _model = value;
                NotifyPropertyChanged(nameof(Model));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
