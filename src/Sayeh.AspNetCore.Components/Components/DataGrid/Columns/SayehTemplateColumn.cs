// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;

namespace Sayeh.AspNetCore.Components;

/// <summary>
/// Represents a <see cref="FluentDataGrid{TGridItem}"/> column whose cells render a supplied template.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the grid.</typeparam>
/// <typeparam name="TValue">The type of property for sorting.</typeparam>
public class SayehTemplateColumn<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TItem, TValue> : SayehColumnBase<TItem>, ISortableColumn<TItem, TValue>, IFilterableColumn<TItem, TValue>, IEditableColumn<TItem> where TItem : class
{
    private static readonly RenderFragment<TItem> EmptyChildContent = _ => builder => { };

    private TItem? _currentItem;
    private PropertyInfo? _editPropertyInfo;

    private TemplateColumnEditContext<TItem, TValue>? _editContext;


    protected override void OnParametersSet()
    {
        if (_sortProvider is null && IsSortableByDefault())
            _sortProvider = new SortProvider();
        else if (FilterProperty is not null && FilterProperty.Body is MemberExpression memberExpression)
            PropertyInfo = typeof(TItem).GetProperty(memberExpression.Member.Name)!;
        base.OnParametersSet();
    }


    /// <summary>
    /// Specifies the content to be rendered for each row in the table.
    /// </summary>
    [Parameter] public RenderFragment<TItem> ChildContent { get; set; } = EmptyChildContent;

    /// <summary>
    /// Specifies the content to be rendered for each row in the table.
    /// </summary>
    [Parameter] public RenderFragment<TemplateColumnEditContext<TItem, TValue>> EditContent { get; set; } = _ => builder => { };

    [Parameter] public bool IsReadonly { get; set; }

    /// <inheritdoc />
    protected internal override void CellContent(RenderTreeBuilder builder, TItem item)
        => builder.AddContent(0, ChildContent(item));

    /// <inheritdoc />
    public void CellEditContent(RenderTreeBuilder builder)
    {
        if (EditContent is not null && _editContext is not null)
            builder.AddContent(0, EditContent(_editContext));
        else
            builder.AddContent(0, ChildContent(_currentItem!));
    }

    /// <inheritdoc />
    internal override bool IsSortableByDefault()
        => SortProperty is not null && base.IsSortableByDefault();

    /// <inheritdoc />
    internal override bool IsFilterableByDefault()
        => FilterProperty is not null;

    #region Implement ISortableColumn

    [Parameter]
    public Expression<Func<TItem, TValue>>? SortProperty { get; set; }

    public Nullable<ListSortDirection> SortDirection { get; set; }

    public short SortOrder { get; set; }

    IEnumerable<TItem> ISortableColumn<TItem>.ApplySort(IEnumerable<TItem> Source, bool IsFirst)
    {
        return _sortProvider!.ApplySort(this, Source, IsFirst);
    }

    #endregion

    #region Implement IFilterableColumn

    //this is used to filter purpose and donot use for other
    public PropertyInfo? PropertyInfo { get; private set; }

    [Parameter]
    public string? EditorPropertyPath { get; set; }

    [Parameter]
    public Expression<Func<TItem, TValue>>? FilterProperty { get; set; }

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

    public async override void SetFocuse()
    {
        try
        {
            if (_editContext is not null && _editContext.Element.Context is not null)
                await _editContext.Element.FocusAsync();
        }
        catch (Exception)
        {

        }
    }

    public object? GetCurrentValue()
    {
        if (_editContext is not null)
            return _editContext.Value;
        else
            return null;
    }

    public void UpdateSource()
    {
        if (!IsReadonly && _currentItem is not null && _editPropertyInfo is not null && _editContext is not null)
            _editPropertyInfo.SetValue(_currentItem, _editContext.Value);
    }

    public void BeginEdit(TItem item)
    {
        _currentItem = item;
        if (EditorPropertyPath is not null)
        {
            _editPropertyInfo = _currentItem.GetType().GetProperty(EditorPropertyPath);
            if (_editPropertyInfo is not null)
                _editContext = new TemplateColumnEditContext<TItem, TValue>(_currentItem, (TValue)_editPropertyInfo.GetValue(_currentItem)!);
        }
    }

    public void CancelEdit()
    {
        _currentItem = null;
        _editPropertyInfo = null;
    }

    public string? GetEditPropertyPath()
    {
        return EditorPropertyPath;
    }

}
