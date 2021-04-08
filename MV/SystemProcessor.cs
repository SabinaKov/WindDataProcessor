using System;
using System.Collections.Generic;
using System.Text;

namespace MV
{
    public class SystemProcessor
    {
        public static object GetElapsedTimeSinceApplicationStarted()
        {
            return DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        }
    }
}
