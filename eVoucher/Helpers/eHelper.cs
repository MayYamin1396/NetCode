using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eVoucher.Helpers
{
    public static class eHelper
    {
        public static void InitializeThread(Thread thread)
        {
            thread.IsBackground = true;
            thread.Start();
        }
        public static long epochTime()
        {
            var currentDate = DateTime.UtcNow;
            var epoch = currentDate - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)epoch.TotalSeconds;
        }
    }
}
