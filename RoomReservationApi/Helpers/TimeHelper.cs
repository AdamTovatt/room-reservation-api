using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RoomReservationApi.Helpers
{
    public class TimeHelper
    {
        private static bool hasCache = false;
        private static DateTimeOffset cacheValue;

        public static DateTimeOffset GetCachedSwedenTime()
        {
            if (!hasCache)
            {
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "W. Europe Standard Time" : "Europe/Stockholm";

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                cacheValue = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);
                hasCache = true;
            }

            return cacheValue;
        }

        public static void ClearCachedTime()
        {
            hasCache = false;
        }
    }
}
