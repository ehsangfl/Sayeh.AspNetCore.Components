using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sample.Client.Model
{

    public class WeatherForeacast
    {

        public static IEnumerable<WeatherForeacast> GenerateItems()
        {
            return new List<WeatherForeacast> {
                new WeatherForeacast(){ Order =1,  Date = DateTime.Now, DayofWeek = "the today", Tempreture = -5 , Type = DayType.Snowy,Description = "no reason" },
                new WeatherForeacast(){ Order =2, Date = DateTime.Now.AddDays(1), DayofWeek =DateTime.Now.AddDays(1).DayOfWeek.ToString() , Tempreture = -5 , Type = DayType.Snowy },
                new WeatherForeacast(){ Order =3, Date = DateTime.Now.AddDays(2), DayofWeek = DateTime.Now.AddDays(2).DayOfWeek.ToString(), Tempreture = -2 , Type = DayType.Rainy },
                new WeatherForeacast(){ Order =4, Date = DateTime.Now.AddDays(3), DayofWeek = DateTime.Now.AddDays(3).DayOfWeek.ToString(), Tempreture = 10 , Type = DayType.Sunny },
                new WeatherForeacast(){ Order =5, Date = DateTime.Now.AddDays(4), DayofWeek = DateTime.Now.AddDays(4).DayOfWeek.ToString(), Tempreture = 5 , Type = DayType.Sunny },
                new WeatherForeacast(){ Order =4, Date = DateTime.Now.AddDays(5), DayofWeek = DateTime.Now.AddDays(5).DayOfWeek.ToString(), Tempreture = 3 , Type = DayType.Rainy}
    };
        }

        public WeatherForeacast()
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
        public double TemporatureF => (Tempreture * 9.5) + 32;
        public string? DayofWeek { get; set; }
        [Required]
        public string? Description { get; set; }

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
