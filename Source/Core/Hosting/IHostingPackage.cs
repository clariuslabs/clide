using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Clide.Hosting
{
    public interface IHostingPackage
    {
        ICompositionService Composition { get; }
    }
}
