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
    public PropertyInfo? PropertyInfo { get; private set; }

    /// <inheritdoc />
    [Parameter, EditorRequired] public Expression<Func<TItem, TValue>> Property { get; set; } = default!;

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
		// We have to do a bit of pre-processing on the lambda expression. Only do that if it's new or changed.
		if (_lastAssignedProperty != Property)
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
            {
                PropertyInfo = memberExpression.Member as PropertyInfo;
                if (Title is null)
                {
                    var daText = memberExpression.Member.DeclaringType?.GetDisplayAttributeString(memberExpression.Member.Name);
                    if (!string.IsNullOrEmpty(daText))
                        Title = daText;
                    else
                        Title = memberExpression.Member.Name;
                }
            }
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
		=> builder.AddContent(0, _cellTextFunc!(item));

    public override void SetFocuse()
    {

    }

}
