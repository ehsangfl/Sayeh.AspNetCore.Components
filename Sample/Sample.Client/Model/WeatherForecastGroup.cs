namespace Sample.Client.Model
{
    public class WeatherForecastGroup : NestedItem
    {
        public WeatherForecastGroup(string name)
        {
            Name = name;
            ID = Guid.NewGuid();
        }

        public Guid ID { get; set; }

        public string Name { get; set; }
        public IEnumerable<WeatherForecast> Items { get; set; }

    }
}
