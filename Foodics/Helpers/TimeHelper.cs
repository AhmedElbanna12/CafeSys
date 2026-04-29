namespace Foodics.Helpers
{
        public static class TimeHelper
        {
            private static readonly TimeZoneInfo CairoZone =
                TimeZoneInfo.GetSystemTimeZones()
                    .FirstOrDefault(tz => tz.Id == "Africa/Cairo" || tz.Id == "Egypt Standard Time")
                ?? TimeZoneInfo.Utc;

            public static DateTime NowCairo() =>
                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CairoZone);
        }
    }
