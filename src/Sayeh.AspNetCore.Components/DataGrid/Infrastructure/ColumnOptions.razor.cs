// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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

    private async void openPopupClicked()
    {
        if (!dropDownIsOpen)
        {
            if (await Owner.Grid.ShowColumnOptionsAsync(OwnerColumn))
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
        else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
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
        if (DistinctList is not null || COwnerColumn is null)
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

            // use the bindingExpression (it already represents the property access).
            // ensure constants use the same type as the binding expression to avoid mismatched types.
            var targetType = bindingExpression.Type;
            var RightExp = Expression.Constant(Convert.ChangeType(value, targetType), targetType);

            Expression result;
            if (targetType == typeof(string))
            {
                var propertyExpression = bindingExpression; // re-use previously captured expression
                var nullCheck = Expression.NotEqual(propertyExpression, Expression.Constant(null, targetType));
                // remove spaces + tolower
                var LeftExp = Expression.Call(propertyExpression, ToLowerMethod);
                LeftExp = Expression.Call(LeftExp, ReplaceMethod, new ConstantExpression[] { OneSpace, Empty });
                result = Expression.AndAlso(nullCheck, Expression.Call(LeftExp, ContainsMethod, new ConstantExpression[] { RightExp }));
            }
            else
            {
                // compare using the bindingExpression (already built safely for dynamic properties)
                result = Expression.Equal(bindingExpression, RightExp);
            }

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
            Expression? Result = null;
            Expression? nullCheck = null;

            var targetType = bindingExpression.Type;
            var isNullable = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
            Expression valueExpr = bindingExpression;
            Type compareType = targetType;
            if (isNullable)
            {
                compareType = Nullable.GetUnderlyingType(targetType)!;
                // access .Value for comparisons
                valueExpr = Expression.Property(bindingExpression, "Value");
                nullCheck = Expression.NotEqual(bindingExpression, Expression.Constant(null, targetType));
            }
            else
            {
                nullCheck = Expression.Constant(true);
            }

            if (fromValue is not null)
            {
                var fromExpr = Expression.Constant(Convert.ChangeType(fromValue, compareType), compareType);
                Result = Expression.AndAlso(nullCheck, Expression.GreaterThanOrEqual(valueExpr, fromExpr));
            }
            if (toValue is not null)
            {
                var toExpr = Expression.Constant(Convert.ChangeType(toValue, compareType), compareType);
                var toWhereC = Expression.AndAlso(nullCheck, Expression.LessThanOrEqual(valueExpr, toExpr));
                if (Result is null)
                    Result = toWhereC;
                else
                    Result = Expression.AndAlso(Result, toWhereC);
            }

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

        if (bindingExpression is null)
            return null;

        // Find any ParameterExpression used inside the bindingExpression.
        // This covers MemberExpression, InvocationExpression, Call, Convert, etc.
        var finder = new ParameterFinder();
        finder.Visit(bindingExpression);
        if (finder.Found is not null)
            return finder.Found;

        // If none found, create a fresh parameter (expression body may not need one).
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


    /// <summary>
    /// Visits an expression tree and captures the first ParameterExpression encountered.
    /// </summary>
    private class ParameterFinder : ExpressionVisitor
    {
        public ParameterExpression? Found { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (Found is null)
                Found = node;
            return base.VisitParameter(node);
        }
    }
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
