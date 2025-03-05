using Microsoft.AspNetCore.Components;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using System.Linq.Expressions;
using System.Reflection;

namespace Sayeh.AspNetCore.Components;

partial class ColumnOptions<TItem, TValue> : ColumnOptionsBase<TItem> where TItem : class
{

    #region Fileds

    bool hasFilter { get; set; } = false;
    Expression? bindingExpression = null;

    private Type propertyType = default!;
    private bool IsNullable = false;
    private bool IsSortable = false;
    MethodInfo ReplaceMethod;
    MethodInfo ToLowerMethod;
    MethodInfo ContainsMethod;
    ConstantExpression OneSpace;
    ConstantExpression Empty;

    #endregion

    #region Initialization

    public ColumnOptions()
    {
        ReplaceMethod = typeof(string).GetMethod("Replace", new[] { typeof(string), typeof(string) })!;
        ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
        ToLowerMethod = typeof(string).GetMethod("ToLower", new Type[0])!;

        OneSpace = Expression.Constant(" ");
        Empty = Expression.Constant("");
    }

    #endregion

    #region Properties

    [CascadingParameter]
    private InternalGridContext<TItem> Owner { get; set; } = default!;

    [CascadingParameter]
    private SayehColumnBase<TItem> OwnerColumn { get; set; } = default!;

    private IFilterableColumn<TItem, TValue> COwnerColumn { get; set; } = default!;


    #region Filters

    public string? TextFilter { get; set; }

    public Nullable<DateTime> FromDate { get; set; }

    public Nullable<DateTime> ToDate { get; set; }

    public Nullable<double> FromNumber { get; set; }

    public Nullable<double> ToNumber { get; set; }

    public Nullable<bool> Bool { get; set; }

    private IEnumerable<TValue>? DistinctList { get; set; }

    private TValue? SelectedItem { get; set; }

    #endregion


    #endregion

    #region Function

    private void openPopupClicked()
    {
        if (!dropDownIsOpen)
        {
            Owner.Grid.ShowColumnOptionsAsync(OwnerColumn);
            dropDownIsOpen = true;
        }
        else
            Owner.Grid.CloseColumnOptions();
    }

    internal override IEnumerable<TItem> ApplyFilter(IEnumerable<TItem> Source)
    {
        if (propertyType is null)
            return Source;
        if (propertyType == typeof(string))
            return applyTextFilter(Source);
        else if (propertyType == typeof(DateTime))
            return applyDateTimeFilter(Source);
        else if (propertyType == typeof(int) || propertyType == typeof(short) || propertyType == typeof(decimal) || propertyType == typeof(double)
                || propertyType == typeof(Single))
            return applyNumberFilter(Source);
        else if (propertyType == typeof(bool))
            return applyBoolFilter(Source);
        else if (propertyType.IsClass)
            return applySelectedItemFilter(applyTextFilter(Source));
        else
        {
            hasFilter = false;
            return Source;
        }
    }

    private void getPropertyType()
    {
        if (OwnerColumn is null)
            return;
        propertyType = typeof(TValue);
        if (typeof(Nullable<>).IsAssignableFrom(propertyType))
        {
            propertyType = propertyType.GetGenericArguments()[0];
            IsNullable = true;
        }
        if (OwnerColumn.Sortable.HasValue ? OwnerColumn.Sortable.Value : OwnerColumn.IsSortableByDefault())
            IsSortable = true;
        COwnerColumn = (IFilterableColumn<TItem, TValue>)OwnerColumn;
    }

    private void prepaireDistictList()
    {
        if (COwnerColumn is null)
            return;
        var filterFunc = COwnerColumn.FilterProperty?.Compile();
        if (filterFunc is not null)
            DistinctList = Owner.Grid.Items?.Select(s => filterFunc(s)).Distinct();
    }

    private void RemoveSort()
    {
        if (typeof(ISortableColumn<TItem>).IsAssignableFrom(OwnerColumn.GetType()))
        {
            var ccol = (ISortableColumn<TItem>)OwnerColumn;
            if (ccol is not null)
            {
                ccol.SortDirection = null;
                ccol.SortOrder = 0;
                Owner.Grid.ApplySort(ccol);
                Owner.Grid.CloseColumnOptions();
            }
        }
    }

    private void ApplySort()
    {
        if (typeof(ISortableColumn<TItem>).IsAssignableFrom(OwnerColumn.GetType()))
        {
            var ccol = (ISortableColumn<TItem>)OwnerColumn;
            if (ccol is not null)
            {
                ccol.SortDirection = System.ComponentModel.ListSortDirection.Ascending;
                Owner.Grid.ApplySort(ccol);
                Owner.Grid.CloseColumnOptions();
            }
        }
    }

    private void ApplySortDescending()
    {
        if (typeof(ISortableColumn<TItem>).IsAssignableFrom(OwnerColumn.GetType()))
        {
            var ccol = (ISortableColumn<TItem>)OwnerColumn;
            if (ccol is not null)
            {
                ccol.SortDirection = System.ComponentModel.ListSortDirection.Descending;
                Owner.Grid.ApplySort(ccol);
                Owner.Grid.CloseColumnOptions();
            }
        }
    }

    private void RemoveFilter()
    {
        TextFilter = null;
        FromDate = null;
        ToDate = null;
        FromNumber = null;
        ToNumber = null;
        SelectedItem = default(TValue);
        Bool = null;
        Owner.Grid.ApplyFilter(COwnerColumn);
        Owner.Grid.CloseColumnOptions();
    }

    #region Filter Request

    private void OnTextFilterSetted(ChangeEventArgs e)
    {
        if (e.Value is not null)
            TextFilter = e.Value.ToString();
        else
            TextFilter = null;
        Owner.Grid.ApplyFilter(COwnerColumn);
        Owner.Grid.CloseColumnOptions();
    }

    private void DateRangeSelected((Nullable<DateTime> fromDate, Nullable<DateTime> toDate) arg)
    {
        FromDate = arg.fromDate;
        ToDate = arg.toDate;
        Owner.Grid.ApplyFilter(COwnerColumn);
        Owner.Grid.CloseColumnOptions();
    }

    private void NumberRangeSelected((Nullable<double> fromDate, Nullable<double> toDate) arg)
    {
        FromNumber = arg.fromDate;
        ToNumber = arg.toDate;
        Owner.Grid.ApplyFilter(COwnerColumn);
        Owner.Grid.CloseColumnOptions();
    }

    private void OnBoolSelectedChanged(string e)
    {
        if (bool.TryParse(e, out bool tmpBool))
            Bool = tmpBool;
        else
            Bool = null;
        Owner.Grid.ApplyFilter((OwnerColumn as IFilterableColumn<TItem>)!);
        Owner.Grid.CloseColumnOptions();
    }

    private void OnSelectedItemChanged(TValue selectedItem)
    {
        this.SelectedItem = selectedItem;
        Owner.Grid.ApplyFilter(COwnerColumn);
        Owner.Grid.CloseColumnOptions();
    }

    #endregion

    #region Filters Apply

    private IEnumerable<TItem> applyTextFilter(IEnumerable<TItem> source)
    {
        if (string.IsNullOrEmpty(TextFilter))
        {
            hasFilter = false;
            return source;
        }
        hasFilter = true;
        var fn = CreateWhereClause(source, TextFilter.Replace(" ", "").ToLower());
        return source.Where(w => fn(w));
    }

    private IEnumerable<TItem> applyDateTimeFilter(IEnumerable<TItem> source)
    {
        if ((!FromDate.HasValue || FromDate == default) && (!ToDate.HasValue || ToDate == default))
        {
            hasFilter = false;
            return source;
        }
        hasFilter = true;
        object? fromValue = null;
        object? toValue = null;
        if (FromDate.HasValue && FromDate != default)
            fromValue = FromDate.Value;
        if (ToDate.HasValue && ToDate != default)
            toValue = ToDate.Value;
        var fn = CreateRangeWhereClause(source, fromValue, ToDate);
        return source.Where(w => fn(w));
    }

    private IEnumerable<TItem> applyNumberFilter(IEnumerable<TItem> source)
    {
        if ((!FromNumber.HasValue || double.IsNaN(FromNumber.Value)) && (!ToNumber.HasValue || double.IsNaN(ToNumber.Value)))
        {
            hasFilter = false;
            return source;
        }
        object? fromValue = null;
        object? toValue = null;
        if (FromNumber.HasValue && !double.IsNaN(FromNumber.Value))
            fromValue = FromNumber.Value;
        if (ToNumber.HasValue && !double.IsNaN(ToNumber.Value))
            toValue = ToNumber.Value;
        hasFilter = true;
        var fn = CreateRangeWhereClause(source, fromValue, toValue);
        return source.Where(w => fn(w));
    }

    private IEnumerable<TItem> applyBoolFilter(IEnumerable<TItem> source)
    {
        if (!IsNullable && !Bool.HasValue)
        {
            hasFilter = false;
            return source;
        }
        hasFilter = true;
        var fn = CreateWhereClause(source, Bool!.Value);
        return source.Where(w => fn(w));
    }

    private IEnumerable<TItem> applySelectedItemFilter(IEnumerable<TItem> source)
    {
        if (SelectedItem is null || DistinctList is null)
        {
            hasFilter = false;
            return source;
        }
        hasFilter = true;
        var fn = CreateWhereClause(source, SelectedItem);
        return source.Where(w => fn(w));
    }

    private Func<TItem, bool> CreateWhereClause(IEnumerable<TItem> source, object value)
    {

        try
        {
            var pInfo = COwnerColumn.PropertyInfo!;
            var pExpression = GetTypeParameterExpressionName();
            if (pExpression is null || bindingExpression is null)
                return Ok();

            //ParameterExpression ExpParam = Expression.Parameter(typeof(TGridItem), "w");
            var RightExp = Expression.Constant(value, pInfo.PropertyType);

            Expression result;
            if (pInfo.PropertyType == typeof(string))
            {
                var propertyExpression = Expression.Property(pExpression, pInfo);
                var nullCheck = Expression.NotEqual(propertyExpression, Expression.Constant(null, pInfo.PropertyType));
                //remove spaces
                var LeftExp = Expression.Call(propertyExpression, ToLowerMethod);
                //run tolower method
                LeftExp = Expression.Call(LeftExp, ReplaceMethod, new ConstantExpression[] { OneSpace, Empty });
                result = Expression.AndAlso(nullCheck, Expression.Call(LeftExp, ContainsMethod, new ConstantExpression[] { RightExp }));
            }
            else
            {
                //var LeftExp = (Expression)Expression.Property(ExpParam, pInfo);
                result = Expression.Equal(bindingExpression, RightExp);
            }
            
            //return Expression.Call(typeof(Queryable), "Where", new Type[] { source.ElementType }, source.Expression, Expression.Lambda(Deleg, Result, new ParameterExpression[] { ExpParam }));
            return Expression.Lambda<Func<TItem, bool>>(result, new ParameterExpression[] { pExpression }).Compile();
        }
        catch (Exception)
        {
            return Ok();
        }
    }

    private Func<TItem, bool> CreateRangeWhereClause(IEnumerable<TItem> source, object? fromValue, object? toValue)
    {
        try
        {
            var pExpression = GetTypeParameterExpressionName();
            if (pExpression is null || bindingExpression is null)
                return Ok();

            var pInfo = COwnerColumn.PropertyInfo!;
            //ParameterExpression ExpParam = Expression.Parameter(typeof(TGridItem), pName);

            Expression? Result = null;
            var nullCheck = Expression.NotEqual(Expression.Property(pExpression, pInfo), Expression.Constant(null, typeof(object)));
            if (fromValue is not null)
            {
                var fromExpr = Expression.Constant(Convert.ChangeType(fromValue, pInfo.PropertyType), pInfo.PropertyType);
                Result = Expression.AndAlso(nullCheck, Expression.GreaterThanOrEqual(bindingExpression, fromExpr));
            }
            if (toValue is not null)
            {
                var toExpr = Expression.Constant(Convert.ChangeType(toValue, pInfo.PropertyType), pInfo.PropertyType);
                var toWehreC =Expression.AndAlso(nullCheck, Expression.LessThanOrEqual(bindingExpression, toExpr));
                if (Result is null)
                    Result = toWehreC;
                else
                    Result = Expression.AndAlso(Result, toWehreC);
            }
            //var Deleg = typeof(Func<,>).MakeGenericType(source.ElementType, typeof(bool));
            //return Expression.Call(typeof(Queryable), "Where", new Type[] { source.ElementType }, source.Expression, Expression.Lambda(Deleg, Result!, new ParameterExpression[] { ExpParam }));
            return Expression.Lambda<Func<TItem, bool>>(Result!, new[] { pExpression }).Compile();

        }
        catch (Exception)
        {
            return Ok();
        }
    }

    private ParameterExpression? GetTypeParameterExpressionName()
    {
        GetBindignExpression();
        if (bindingExpression is MemberExpression)
        {
            var paramExpression = ((MemberExpression)bindingExpression).Expression;
            while (paramExpression is MemberExpression)
            {
                paramExpression = ((MemberExpression)paramExpression).Expression;
            }
            if (paramExpression is ParameterExpression)
                return ((ParameterExpression)paramExpression);
        }
        return Expression.Parameter(typeof(TItem), "w");
    }

    private void GetBindignExpression()
    {
        if (bindingExpression is null)
        {
            if (COwnerColumn.FilterProperty is not null)
                bindingExpression = COwnerColumn.FilterProperty.Body;
            else if (typeof(IBindableColumn).IsAssignableFrom(OwnerColumn.GetType()) && ((IBindableColumn<TItem, TValue>)OwnerColumn).Property is not null)
                bindingExpression = ((IBindableColumn<TItem, TValue>)OwnerColumn).Property.Body;
        }
    }

    private Func<TItem, bool> Ok()
    {
        return _ => true;
    }

    #endregion

    #endregion

}

public abstract class ColumnOptionsBase<TGridItem> : ComponentBase
{
    #region Fileds

    protected bool dropDownIsOpen { get; set; } = false;

    #endregion

    internal void CloseDropDown()
    {
        dropDownIsOpen = false;
        StateHasChanged();
    }

    internal abstract IEnumerable<TGridItem> ApplyFilter(IEnumerable<TGridItem> Source);

}
