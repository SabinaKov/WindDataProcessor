using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindDataProcessing
{
    internal class LoadCase
    {
        internal string Name { get; set; }
        internal double TimeShare { get; set; }
        internal List<LoadState> LoadStates { get; set; }
    }
}
