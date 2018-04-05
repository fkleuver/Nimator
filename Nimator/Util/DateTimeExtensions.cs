using System;

// ReSharper disable MemberCanBePrivate.Global
namespace Nimator.Util
{

    public static class DateTimeExtensions
    {
        internal static readonly DateTime WindowsFileTimeEpoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly DateTime GregorianCalendarEpoch = new DateTime(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Uses the TimeZoneInfo for conversion, which will throw on historically invalid DateTimes.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static DateTime RobustToUTC(this DateTime dateTime)
        {
            // See: https://stackoverflow.com/questions/1704780/what-is-the-difference-between-datetime-touniversaltime-and-timezoneinfo-convert
            return TimeZoneInfo.ConvertTimeToUtc(dateTime);
        }

        /// <summary>
        /// Substracts the Windows File Time epoch (1601-01-01) from the provided DateTime (which has an earlier epoch),
        /// such that the result is the amount of time passed since the Windows File Time epoch.
        /// </summary>
        public static TimeSpan MinusWindowsFileTimeEpoch(this DateTime dateTime)
        {
            return RobustToUTC(dateTime).Subtract(WindowsFileTimeEpoch);
        }
        
        /// <summary>
        /// Substracts the Unix epoch (1970-01-01) from the provided DateTime (which has an earlier epoch),
        /// such that the result is the amount of time passed since the Unix epoch.
        /// </summary>
        public static TimeSpan MinusUnixEpoch(this DateTime dateTime)
        {
            return RobustToUTC(dateTime).Subtract(UnixEpoch);
        }
        
        /// <summary>
        /// Substracts the Gregorian Calendar epoch (1582-10-15) from the provided DateTime (which has an earlier epoch),
        /// such that the result is the amount of time passed since the Gregorian Calendar epoch.
        /// </summary>
        public static TimeSpan MinusGregorianCalendarEpoch(this DateTime dateTime)
        {
            return RobustToUTC(dateTime).Subtract(GregorianCalendarEpoch);
        }
    }
}
