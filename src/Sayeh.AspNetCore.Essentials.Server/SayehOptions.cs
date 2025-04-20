using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Essentials.Server
{
    public class SayehOptions
    {
        public Localization Localization { get; set; } = new();
    }

    public class Localization
    {
        public string[] SupportedCultures { get; set; } = new string[0];
        public string CultureClaimType { get; set; } = "Culture";
        public bool BindDefaultCulture { get; set; }
    }
}
