using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindDataProcessing
{
    internal class LoadCase
    {
        public int NumberOfLoadStates { get; internal set; }
        public double AverageSpeed { get; internal set; }
        internal string Name { get; set; }
        internal double TimeShare { get; set; }
        internal List<LoadState> LoadStates { get; set; }
    }
}
