using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayeh.Essentials.Core;

public interface ILocalizationManager
{

    public Task ChangeCulture(string NewCulture);

    public ValueTask<string> GetCurrentCulture();

    public void SetCulture(string Culture);
    
}
