﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindDataProcessing
{
    internal class LoadState
    {
        /// <summary>
        /// FX [N]
        /// </summary>
        internal double FX { get; set; }

        /// <summary>
        /// FY [N]
        /// </summary>
        internal double FY { get; set; }

        /// <summary>
        /// FZ [N]
        /// </summary>
        internal double FZ { get; set; }
        /// <summary>
        /// MX [Nm]
        /// </summary>
        internal double MX { get; set; }
        /// <summary>
        /// MY [Nm]
        /// </summary>
        internal double MY { get; set; }

        /// <summary>
        /// MZ [Nm]
        /// </summary>
        internal double MZ { get; set; }

        /// <summary>
        /// Speed [rev / min]
        /// </summary>
        internal double Speed { get; set; }

        internal BearingState FMBState { get; set; }

        internal BearingState RMBState { get; set; }
    }
}