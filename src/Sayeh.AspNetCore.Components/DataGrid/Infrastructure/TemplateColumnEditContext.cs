using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components.DataGrid.Infrastructure
{
    public class TemplateColumnEditContext<TGridItem,TValue> where TGridItem : class
    {

        public TGridItem Item { get; set; }

        public TValue? Value { get; set; }

        public ElementReference Element { get; set; }

        public TemplateColumnEditContext(TGridItem item, TValue value)
        {
            Item = item;
            Value = value;
        }

    }
}
