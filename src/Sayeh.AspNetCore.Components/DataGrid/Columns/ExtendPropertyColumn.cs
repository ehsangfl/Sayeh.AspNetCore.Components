using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using Sayeh.AspNetCore.Components.Infrastructure;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Sayeh.AspNetCore.Components;

partial class ExtendPropertyColumn<TItem, TValue, TSort, TFilter>
: SayehColumnBase<TItem>
    , IBindableColumn<TItem, TValue>
    , ISortableColumn<TItem, TSort>
    , IFilterableColumn<TItem, TFilter>
where TItem : class
{

    public ExtendPropertyColumn()
    {
        this.Sortable = true;
        this.Filterable = true;
    }

    private Expression<Func<TItem, TValue>>? _lastAssignedProperty;
    private Func<TItem, TValue>? _compiledProperty;
    private Func<TItem, string?>? _cellTextFunc;

    /// <summary>
    /// Defines the value to be displayed in this column's cells.
    /// </summary>
    [Parameter, EditorRequired]
    public Expression<Func<TItem, TValue>> Property { get; set; } = default!;

    /// <summary>
    /// Optionally specifies a format string for the value.
    ///
    /// Using this requires the <typeparamref name="TValue"/> type to implement <see cref="IFormattable" />.
    /// </summary>
    [Parameter] public string? Format { get; set; }

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
                if (typeof(IFormattable).IsAssignableFrom(typeof(TValue)))
                {
                    _cellTextFunc = item => ((IFormattable?)_compiledProperty!(item))?.ToString(Format, null);

                }
                else
                {
                    throw new InvalidOperationException($"A '{nameof(Format)}' parameter was supplied, but the type '{typeof(TValue)}' does not implement '{typeof(IFormattable)}'.");
                }
            }
            else if (Converter is not null)
            {
                _cellTextFunc = item => Converter.Invoke(_compiledProperty!(item));
            }
            else
            {
                _cellTextFunc = item => _compiledProperty!(item)?.ToString();
            }
        }

        if (Property.Body is MemberExpression memberExpression)
        {
            PropertyInfo = typeof(TItem).GetProperty(memberExpression.Member.Name);
            if (PropertyInfo is null || PropertyInfo.DeclaringType is null)
                return;
            if (Title is null)
            {
                var daText = PropertyInfo.DeclaringType.GetDisplayAttributeString(memberExpression.Member.Name);
                if (!string.IsNullOrEmpty(daText))
                    Title = daText;
                else
                    Title = memberExpression.Member.Name;
            }
        }
    }

    /// <inheritdoc />
    protected internal override void CellContent(RenderTreeBuilder builder, TItem item)
        => builder.AddContent(10, _cellTextFunc!(item));


    #region Implement ISortableColumn

    [Parameter]
    public Expression<Func<TItem, TSort>>? SortProperty { get; set; }

    public Nullable<ListSortDirection> SortDirection { get; set; }

    public PropertyInfo? PropertyInfo { get; private set; }

    public short SortOrder { get; set; }

    IEnumerable<TItem> ISortableColumn<TItem>.ApplySort(IEnumerable<TItem> Source, bool IsFirst)
    {
        return _sortProvider!.ApplySort(this, Source, IsFirst);
    }


    #endregion

    #region Implement IFilterableColumn

    [Parameter]
    public Expression<Func<TItem, TFilter>>? FilterProperty { get; set; }

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


    #endregion

    public override void SetFocuse()
    {

    }

}
