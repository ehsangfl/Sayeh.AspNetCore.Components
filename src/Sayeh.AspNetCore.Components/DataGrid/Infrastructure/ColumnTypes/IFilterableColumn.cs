using Microsoft.FluentUI.AspNetCore.Components;
using System.Linq.Expressions;
using System.Reflection;

namespace Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

internal interface IFilterableColumn<TItem> 
{
    public IEnumerable<TItem> ApplyFilter(IEnumerable<TItem> Source);

    public PropertyInfo? PropertyInfo { get; }
}

internal interface IFilterableColumn<TItem, TValue> : IFilterableColumn<TItem>
{

    public Expression<Func<TItem, TValue>>? FilterProperty { get; set; }

}
internal static class IFilterableColumnExtensions
{

    public static bool FilterIsEnable<TItem>(this SayehColumnBase<TItem> col) where TItem : class
    {
        return col.IsFilterableByDefault() && (!col.Filterable.HasValue || col.Filterable.Value);
    }

}