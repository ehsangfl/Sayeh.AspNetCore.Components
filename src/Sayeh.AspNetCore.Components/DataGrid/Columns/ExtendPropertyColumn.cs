using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using Sayeh.AspNetCore.Components.Infrastructure;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Sayeh.AspNetCore.Components;

partial class ExtendPropertyColumn<TItem, TValue, TSort, TFilter>
: SayehPropertyColumn<TItem,TValue>
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

    #region Implement ISortableColumn

    [Parameter]
    public new Expression<Func<TItem, TSort>>? SortProperty { get; set; }

    IEnumerable<TItem> ISortableColumn<TItem>.ApplySort(IEnumerable<TItem> Source, bool IsFirst)
    {
        return _sortProvider!.ApplySort<TItem,TSort>(this, Source, IsFirst);
    }


    #endregion

    #region Implement IFilterableColumn

    [Parameter]
    public new Expression<Func<TItem, TFilter>>? FilterProperty { get; set; }

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
