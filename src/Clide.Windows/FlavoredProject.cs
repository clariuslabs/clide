﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    class FlavoredProject
    {
        public FlavoredProject(IVsHierarchy hierarchy, IVsHierarchy innerHierarchy)
        {
            Hierarchy = hierarchy;
            InnerHierarchy = innerHierarchy;
        }

        public IVsHierarchy Hierarchy { get; }

        public IVsHierarchy InnerHierarchy { get; }
    }
}
