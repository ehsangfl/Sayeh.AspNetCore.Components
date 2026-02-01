using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components
{
    internal static class Extensions
    {
        public static string ToAttributeValue(this bool value) => value ? "true" : "false";
    }
}
