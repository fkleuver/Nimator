using System;
using System.Runtime.InteropServices;

// ReSharper disable MemberCanBePrivate.Global
namespace Nimator.Util
{
    /// <summary>
    /// A DateTime provider which uses the Windows Kernel api to get timestamps with 100-nanosecond precision.
    /// Useful in particular for generating time-based UUID's which also have a 100-nanosecond precision,
    /// or in general for anything that needs a higher precision than the default ~25ms offered by System.DateTime.
    /// </summary>
    public static class DateTimeProvider
    {
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        /// <summary>
        /// Whether the Kernel32.dll GetSystemTimePreciseAsFileTime api is available.
        /// </summary>
        public static bool IsSystemTimePreciseAvailable { get; }

        static DateTimeProvider()
        {
            try
            {
                GetSystemTimePreciseAsFileTime(out _);
                IsSystemTimePreciseAvailable = true;
            }
            catch (EntryPointNotFoundException)
            {
                // Not running Windows 8 or higher.             
                IsSystemTimePreciseAvailable = false;
            }
        }

        /// <summary>
        /// Gets the number of ticks (100-nanosecond intervals) that have elapsed since the Windows File Time
        /// epoch (1601-01-01).
        /// </summary>
        public static long GetSystemTimePreciseAsFileTime()
        {
            if (!IsSystemTimePreciseAvailable)
            {
                throw new InvalidOperationException("High resolution clock isn't available.");
            }
            GetSystemTimePreciseAsFileTime(out var filetime);
            return filetime;
        }

        /// <summary>
        /// Gets the current date and time in 100-nanosecond precision (UTC).
        /// </summary>
        public static DateTime GetSystemTimePrecise()
        {
            var filetime = GetSystemTimePreciseAsFileTime();
            return DateTime.FromFileTimeUtc(filetime);
        }

        /// <summary>
        /// Gets the current date and time (UTC). This is simply a wrapper around <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public static DateTime GetSystemTime()
        {
            return DateTime.UtcNow;
        }
    }
}
