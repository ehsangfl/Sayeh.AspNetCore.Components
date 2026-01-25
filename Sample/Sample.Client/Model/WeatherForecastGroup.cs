namespace Sample.Client.Model
{
    public class WeatherForecastGroup
    {
        public WeatherForecastGroup(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public IEnumerable<WeatherForecast> Items { get; set; }

    }
}
