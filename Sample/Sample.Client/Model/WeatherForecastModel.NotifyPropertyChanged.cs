using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sample.Client.Model
{
    public class WeatherForecastModel_NotifyPropertyChanged : INotifyPropertyChanged
    {
        private Guid iD;
        private bool isSelected;
        private int order;
        private DateTime date;
        private DayType type;
        private int tempreture;
        private string? dayofWeek;
        private string? description;

        public WeatherForecastModel_NotifyPropertyChanged()
        {
            ID = Guid.NewGuid();
        }
        public Guid ID { get => iD; set { iD = value; NotifyPropertyChanged(nameof(ID)); } }
        public bool IsSelected { get => isSelected; set { isSelected = value; NotifyPropertyChanged(nameof(IsSelected)); } }
        public int Order { get => order; set { order = value; NotifyPropertyChanged(nameof(Order)); } }
        public DateTime Date { get => date; set { date = value; NotifyPropertyChanged(nameof(Date)); } }
        public DayType Type { get => type; set { type = value; NotifyPropertyChanged(nameof(Type)); } }
        [Range(0, 200)]
        public int Tempreture { get => tempreture; set { tempreture = value; NotifyPropertyChanged(nameof(Tempreture)); } }
        public double TemporatureF => (Tempreture * 9.5) + 32;
        public string? DayofWeek { get => dayofWeek; set { dayofWeek = value; NotifyPropertyChanged(nameof(DayofWeek)); } }
        [Required]
        public string? Description { get => description; set { description = value; NotifyPropertyChanged(nameof(Description)); } }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
