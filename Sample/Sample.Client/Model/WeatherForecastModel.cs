using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sample.Client.Model
{

    public class WeatherForecast
    {

        public static IList<WeatherForecast> GenerateItems()
        {

            return new List<WeatherForecast> {
                new WeatherForecast(){ Order =1,  Date = DateTime.Now, DayofWeek = "the today", Tempreture = -5 , Type = DayType.Snowy,Description = "no reason" },
                new WeatherForecast(){ Order =2, Date = DateTime.Now.AddDays(1), DayofWeek =DateTime.Now.AddDays(1).DayOfWeek.ToString() , Tempreture = -5 , Type = DayType.Snowy },
                new WeatherForecast(){ Order =3, Date = DateTime.Now.AddDays(2), DayofWeek = DateTime.Now.AddDays(2).DayOfWeek.ToString(), Tempreture = -2 , Type = DayType.Rainy },
                new WeatherForecast(){ Order =4, Date = DateTime.Now.AddDays(3), DayofWeek = DateTime.Now.AddDays(3).DayOfWeek.ToString(), Tempreture = 10 , Type = DayType.Sunny },
                new WeatherForecast(){ Order =5, Date = DateTime.Now.AddDays(4), DayofWeek = DateTime.Now.AddDays(4).DayOfWeek.ToString(), Tempreture = 5 , Type = DayType.Sunny },
                new WeatherForecast(){ Order =6, Date = DateTime.Now.AddDays(5), DayofWeek = DateTime.Now.AddDays(5).DayOfWeek.ToString(), Tempreture = 3 , Type = DayType.Rainy}
    };
        }

        public static IList<WeatherForecastGroup> GenerateGroupedItems()
        {
            var snowGroup = new WeatherForecastGroup(DayType.Snowy.ToString());
            var rainyGroup = new WeatherForecastGroup(DayType.Rainy.ToString());
            var sunnyGroup = new WeatherForecastGroup(DayType.Sunny.ToString());

            var items = new List<WeatherForecast>
            {
                new WeatherForecast() { Order = 1, Date = DateTime.Now, DayofWeek = "the today", Tempreture = -5, Type = DayType.Snowy, Description = "no reason", Group = snowGroup },
                new WeatherForecast() { Order = 2, Date = DateTime.Now.AddDays(1), DayofWeek = DateTime.Now.AddDays(1).DayOfWeek.ToString(), Tempreture = -5, Type = DayType.Snowy, Group = snowGroup },
                new WeatherForecast() { Order = 3, Date = DateTime.Now.AddDays(2), DayofWeek = DateTime.Now.AddDays(2).DayOfWeek.ToString(), Tempreture = -2, Type = DayType.Rainy, Group = rainyGroup },
                new WeatherForecast() { Order = 4, Date = DateTime.Now.AddDays(3), DayofWeek = DateTime.Now.AddDays(3).DayOfWeek.ToString(), Tempreture = 10, Type = DayType.Sunny, Group = sunnyGroup },
                new WeatherForecast() { Order = 5, Date = DateTime.Now.AddDays(4), DayofWeek = DateTime.Now.AddDays(4).DayOfWeek.ToString(), Tempreture = 5, Type = DayType.Sunny, Group = sunnyGroup },
                new WeatherForecast() { Order = 6, Date = DateTime.Now.AddDays(5), DayofWeek = DateTime.Now.AddDays(5).DayOfWeek.ToString(), Tempreture = 3, Type = DayType.Rainy, Group = rainyGroup },
                new WeatherForecast() { Order = 6, Date = DateTime.Now.AddDays(6), DayofWeek = DateTime.Now.AddDays(6).DayOfWeek.ToString(), Tempreture = 3, Type = DayType.Rainy, Group = rainyGroup },
            };

            var types = items.Select(s => s.Group).Distinct();
            var groups = new List<WeatherForecastGroup>();
            foreach (var type in types)
            {
                type.Items = items.Where(w => w.Group == type).ToList();
                groups.Add(type);
            }
            return groups;
        }

        public WeatherForecast()
        {
            ID = Guid.NewGuid();
        }
        public Guid ID { get; set; }
        public bool IsSelected { get; set; }
        public int Order { get; set; }
        public DateTime Date { get; set; }
        public DayType Type { get; set; }
        [Range(0, 200)]
        public int Tempreture { get; set; }
        [Display(Name = "Temprature (F)")]
        public double TemporatureF => (Tempreture * 9.5) + 32;
        public string? DayofWeek { get; set; }
        [Required]
        public string? Description { get; set; }

        public WeatherForecastGroup Group { get; set; }

        public override string ToString()
        {
            return this.Date.ToShortDateString() + "" + Type;
        }

    }


    public enum DayType
    {
        Sunny = 1,
        Rainy = 2,
        Snowy = 3
    }
}
