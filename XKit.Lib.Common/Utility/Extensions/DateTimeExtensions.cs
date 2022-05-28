using System;

namespace XKit.Lib.Common.Utility.Extensions {

    public static class DateTimeExtensions {

        // =====================================================================
        // Public extensios
        // =====================================================================

        public static DateTime FloorTime(this DateTime dt, TimeSpan interval)  {
            return dt.AddTicks(-1 * (dt.Ticks % interval.Ticks));
        }

        // =====================================================================
        // private helpers
        // =====================================================================
    }
}