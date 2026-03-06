// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using Sayeh.AspNetCore.Components.Infrastructure;
namespace Sayeh.AspNetCore.Components;

/// <summary>
/// Represents a <see cref="FluentDataGrid{TGridItem}"/> column whose cells display a single value.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the grid.</typeparam>
/// <typeparam name="TValue">The type of the value being displayed in the column's cells.</typeparam>
public partial class SayehPropertyColumn<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TItem, TValue>
    : SayehColumnBase<TItem>
    , IBindableColumn<TItem, TValue>
    , ISortableColumn<TItem, TValue>
    , IFilterableColumn<TItem, TValue> where TItem : class
{

    #region Feilds

    private Expression<Func<TItem, TValue>>? _lastAssignedProperty;
    protected Func<TItem, TValue>? _compiledProperty;
    protected Func<TItem, string?>? _cellTextFunc;

    #endregion   

    public SayehPropertyColumn()
    {
        this.Sortable = true;
        this.Filterable = true;
    }

    /// <inheritdoc />
    public PropertyInfo? PropertyInfo { get; protected set; }

    /// <inheritdoc />
    [Parameter] public Expression<Func<TItem, TValue>> Property { get; set; } = default!;

    string? _propertyName;
    [Parameter] public string? PropertyName { get; set; }

    /// <summary>
    /// Optionally specifies a format string for the value.
    ///
    /// Using this requires the <typeparamref name="TValue"/> type to implement <see cref="IFormattable" />.
    /// </summary>
    [Parameter] public string? Format { get; set; }


    private Expression<Func<TItem, TValue>>? _sortProperty;
    /// <inheritdoc />
    [Parameter]
    public Expression<Func<TItem, TValue>>? SortProperty
    {
        get
        {
            if (_sortProperty is null && Property is not null)
                return Property;
            return _sortProperty;
        }
        set => _sortProperty = value;
    }

    /// <inheritdoc />
    public ListSortDirection? SortDirection { get; set; }

    /// <inheritdoc />
    public short SortOrder { get; set; }

    [Parameter]
    public Func<TValue, string>? Converter { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        bool titleHasChanged = false;
        if (_propertyName != PropertyName)
        {
            _propertyName = PropertyName;
            if (PropertyName is not null)
            {
                var titemType = typeof(TItem);
                PropertyInfo = titemType.GetProperty(PropertyName);
                if (PropertyInfo is null && titemType.IsA<ICustomTypeProvider>())
                    PropertyInfo = ((ICustomTypeProvider)Activator.CreateInstance(titemType)!)?.GetCustomType().GetProperty(PropertyName);
                if (PropertyInfo is not null)
                    SetPropertyViaExpression();
                else
                    throw new Exception($"Sayeh : object of type {titemType} does not have property name : {PropertyName}");

            }
            else
                Property = null;
            titleHasChanged = true;
        }

        // We have to do a bit of pre-processing on the lambda expression. Only do that if it's new or changed.
        if (_lastAssignedProperty != Property)
        {
            titleHasChanged = true;
            if (Property is not null)
            {
                _lastAssignedProperty = Property;
                _compiledProperty = Property.Compile();

                if (!string.IsNullOrEmpty(Format))
                {
                    // TODO: Consider using reflection to avoid having to box every value just to call IFormattable.ToString
                    // For example, define a method "string Format<U>(Func<TGridItem, U> property) where U: IFormattable", and
                    // then construct the closed type here with U=TProp when we know TProp implements IFormattable

                    // If the type is nullable, we're interested in formatting the underlying type
                    var nullableUnderlyingTypeOrNull = Nullable.GetUnderlyingType(typeof(TValue));
                    if (!typeof(IFormattable).IsAssignableFrom(nullableUnderlyingTypeOrNull ?? typeof(TValue)))
                    {
                        throw new InvalidOperationException($"A '{nameof(Format)}' parameter was supplied, but the type '{typeof(TValue)}' does not implement '{typeof(IFormattable)}'.");
                    }

                    _cellTextFunc = item => ((IFormattable?)_compiledProperty!(item))?.ToString(Format, null);
                }
                else if (Converter is not null)
                {
                    _cellTextFunc = item => Converter.Invoke(_compiledProperty!(item));
                }
                else
                {
                    _cellTextFunc = item => _compiledProperty!(item)?.ToString();
                }
                if (Property.Body is MemberExpression memberExpression)
                    PropertyInfo = memberExpression.Member as PropertyInfo;
            }
            else PropertyInfo = null;
        }
        if (PropertyInfo is null || PropertyInfo.DeclaringType is null || !titleHasChanged)
            return;
        if (Title is null)
        {
            var daText = PropertyInfo.DeclaringType.GetDisplayAttributeString(PropertyInfo.Name);
            if (!string.IsNullOrEmpty(daText))
                Title = daText;
            else
                Title = PropertyInfo.Name;
        }
    }

    IEnumerable<TItem> ISortableColumn<TItem>.ApplySort(IEnumerable<TItem> Source, bool IsFirst)
    {
        return _sortProvider!.ApplySort(this, Source, IsFirst);
    }

    private Expression<Func<TItem, TValue>>? _filterProperty;
    [Parameter]
    public Expression<Func<TItem, TValue>>? FilterProperty
    {
        get
        {
            if (_filterProperty is null && Property is not null)
                return Property;
            return _filterProperty;
        }
        set { _filterProperty = value; }
    }

    IEnumerable<TItem> IFilterableColumn<TItem>.ApplyFilter(IEnumerable<TItem> Source)
    {
        if (FilterProperty is not null)
        {
            if (headerOptionsComponentHolder is not null && headerOptionsComponentHolder.Instance is not null)
            {
                Source = ((ColumnOptionsBase<TItem>)headerOptionsComponentHolder.Instance).ApplyFilter(Source);
            }
            return Source;
        }
        return Source;
    }

    /// <inheritdoc />
    protected internal override void CellContent(RenderTreeBuilder builder, TItem item)
    {
        var value = _cellTextFunc!(item);
        builder.AddContent(0, _cellTextFunc!(item));
    }

    public override void SetFocuse()
    {

    }

    private void SetPropertyViaExpression()
    {
        ArgumentNullException.ThrowIfNull(PropertyInfo);

        // Common parameter for the lambda
        var param = Expression.Parameter(typeof(TItem), "p");

        // If the property is declared on (or assignable from) TItem try to build a direct getter expression.
        // Prefer calling the getter method (if any). This produces the fastest code when possible.
        if (PropertyInfo.DeclaringType is not null && PropertyInfo.DeclaringType.IsAssignableFrom(typeof(TItem)))
        {
            var instanceExpr = (Expression)param;
            if (PropertyInfo.DeclaringType != typeof(TItem))
                instanceExpr = Expression.Convert(param, PropertyInfo.DeclaringType);

            var getter = PropertyInfo.GetMethod;
            if (getter is not null)
            {
                // Build an expression that calls the instance getter directly
                var getCall = Expression.Call(instanceExpr, getter);
                Expression converted = getCall;
                if (getCall.Type != typeof(TValue))
                    converted = Expression.Convert(getCall, typeof(TValue));

                Property = Expression.Lambda<Func<TItem, TValue>>(converted, param);

                // Attempt to create a fast compiled delegate immediately if signatures match exactly
                try
                {
                    if (getter.DeclaringType == typeof(TItem) && getter.ReturnType == typeof(TValue))
                    {
                        // direct strongly-typed delegate: Func<TItem, TValue>
                        _compiledProperty = (Func<TItem, TValue>)Delegate.CreateDelegate(typeof(Func<TItem, TValue>), getter);
                    }
                    // otherwise leave _compiledProperty to be set by the existing compile path below
                }
                catch
                {
                    // ignore if CreateDelegate fails - the normal Property.Compile() path will still work
                }

                return;
            }

            // If there's no getter method (custom PropertyInfo), fall through to fallback below.
        }

        // Fallback for "dynamic" / custom PropertyInfo (e.g. from ICustomTypeProvider):
        // Create a small wrapper delegate once that invokes PropertyInfo.GetValue and converts result.
        // Use Delegate.CreateDelegate to get a direct delegate to PropertyInfo.GetValue (faster than MethodInfo.Invoke).
        var getValueMethod = typeof(PropertyInfo).GetMethod("GetValue", new[] { typeof(object) });
        if (getValueMethod is null)
        {
            // As a last resort, create an expression that throws (shouldn't happen)
            Property = Expression.Lambda<Func<TItem, TValue>>(Expression.Default(typeof(TValue)), param);
            return;
        }

        // Create a fast delegate for PropertyInfo.GetValue: Func<PropertyInfo, object?, object?>
        Func<PropertyInfo, object?, object?> getValueDel;
        try
        {
            getValueDel = (Func<PropertyInfo, object?, object?>)Delegate.CreateDelegate(
                typeof(Func<PropertyInfo, object?, object?>),
                getValueMethod);
        }
        catch
        {
            // If CreateDelegate fails fallback to a delegate that calls Invoke (slower)
            getValueDel = (pi, target) => getValueMethod.Invoke(pi, new object?[] { target });
        }

        // Wrapper that calls the getValue delegate once and converts
        Func<TItem, TValue> wrapper = item =>
        {
            var raw = getValueDel(PropertyInfo, item);
            return raw is null ? default : (TValue)raw;
        };

        // Represent the wrapper in an expression so consumers that expect an Expression<T> still get one.
        // The compiled delegate is the wrapper we just created (fast for subsequent calls).
        Property = Expression.Lambda<Func<TItem, TValue>>(Expression.Invoke(Expression.Constant(wrapper), param), param);
        _compiledProperty = wrapper;
    }
}
