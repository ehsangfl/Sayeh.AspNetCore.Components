using System.Linq.Expressions;

namespace Sayeh.AspNetCore.Components.Test;

[TestClass]
public class ColumnOptionsTest : TestBase
{
    [TestMethod]
    public void ApplyFilterTest_DateTime()
    {
        // Arrange
        var items = WeatherForeacast.GenerateItems();
        var fromDate = DateTime.Now.AddDays(1);
        var toDate = DateTime.Now.AddDays(2);

        // prepare Expression for the property (SayehPropertyColumn expects an Expression<Func<TItem, TValue>>)
        Expression<Func<WeatherForeacast, DateTime>> expression = x => x.Date;
        // Build a ChildContent fragment that adds a SayehPropertyColumn with ColumnOptions host inside.
        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehPropertyColumn<WeatherForeacast, DateTime>));
            builder.AddAttribute(seq++, nameof(SayehPropertyColumn<WeatherForeacast, DateTime>.Property), expression);
            builder.CloseComponent();
        };

        // Render the DataGrid hosting the column (the grid must render so the column can instantiate its header options holder)
        var cut = Render<SayehDataGrid<WeatherForeacast>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent(gridChild)
        );

        // Find the rendered SayehPropertyColumn instance
        var propertyColumn = cut.FindComponent<SayehPropertyColumn<WeatherForeacast, DateTime>>().Instance;

        // Find the ColumnOptions instance that SayehColumnBase rendered via DynamicComponent
        var colOptionsRendered = cut.FindComponent<ColumnOptions<WeatherForeacast, DateTime>>();
        var colOptionsInstance = colOptionsRendered.Instance;

        // Set the filter state directly on the rendered ColumnOptions instance (not parameters).
        // ColumnOptions exposes FromDate/ToDate as ordinary properties (not [Parameter]) so set them on the instance.
        colOptionsInstance.FromDate = fromDate;
        colOptionsInstance.ToDate = toDate;


        // Act
        // Call the explicit IFilterableColumn<TItem>.ApplyFilter implementation on the column.
        var filtered = ((Sayeh.AspNetCore.Components.DataGrid.Infrastructure.IFilterableColumn<WeatherForeacast>)propertyColumn).ApplyFilter(items);

        // Assert
        Assert.IsTrue(filtered.All(item =>
           item.Date >= fromDate
        && item.Date <= toDate),
            "One or more items are outside the expected date range.");
    }

    [TestMethod]
    public void ApplyFilterTest_ICustomeTypeProvider_DateTime()
    {
        // Arrange
        var items = CustomTypeModel.GenerateItems(WeatherForeacast.GenerateItems()).ToList();
        var fromDate = DateTime.Now.AddDays(-5);
        var toDate = DateTime.Now.AddDays(5);

        // Build a ChildContent fragment that adds a SayehPropertyColumn with ColumnOptions host inside.
        RenderFragment gridChild = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(SayehPropertyColumn<CustomTypeModel, DateTime>));
            builder.AddAttribute(seq++,nameof(SayehPropertyColumn<CustomTypeModel, DateTime>.PropertyName) , "DateTimeProperty");
            builder.CloseComponent();
        };

        // Render the DataGrid hosting the column (the grid must render so the column can instantiate its header options holder)
        var cut = Render<SayehDataGrid<CustomTypeModel>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent(gridChild)
        );

        // Find the rendered SayehPropertyColumn instance
        var propertyColumn = cut.FindComponent<SayehPropertyColumn<CustomTypeModel, DateTime>>().Instance;

        // Find the ColumnOptions instance that SayehColumnBase rendered via DynamicComponent
        var colOptionsRendered = cut.FindComponent<ColumnOptions<CustomTypeModel, DateTime>>();
        var colOptionsInstance = colOptionsRendered.Instance;

        // Set the filter state directly on the rendered ColumnOptions instance (not parameters).
        // ColumnOptions exposes FromDate/ToDate as ordinary properties (not [Parameter]) so set them on the instance.
        colOptionsInstance.FromDate = fromDate;
        colOptionsInstance.ToDate = toDate;


        // Act
        // Call the explicit IFilterableColumn<TItem>.ApplyFilter implementation on the column.
        var filtered = ((Sayeh.AspNetCore.Components.DataGrid.Infrastructure.IFilterableColumn<CustomTypeModel>)propertyColumn).ApplyFilter(items);

        // Assert
        Assert.IsTrue(filtered.All(item => 
           DateTime.Parse(item.GetPropertyValue("DateTimeProperty")?.ToString() ?? DateTime.MinValue.ToString()) >= fromDate 
        && DateTime.Parse(item.GetPropertyValue("DateTimeProperty")?.ToString() ?? DateTime.MaxValue.ToString()) <= toDate),
            "One or more items are outside the expected date range.");
    }
}