using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

// ReSharper disable MemberCanBePrivate.Global
namespace Nimator.Util
{
    /// <summary>
    /// Utility for generating Version 1 UUID's (RFC 4122)
    /// </summary>
    public static class GuidGenerator
    {
        private static readonly Random Random;
        private static readonly object Lock = new object();

        private static long _lastTimestampForNoDuplicatesGeneration = GetTicksSinceGregorianCalendarEpoch();
        private static short _lastClockSequenceForNoDuplicatesGeneration;

        // number of bytes in uuid
        private const int ByteArraySize = 16;

        // multiplex variant info
        private const int VariantByte = 8;
        private const int VariantByteMask = 0x3f;
        private const int VariantByteShift = 0x80;

        // multiplex version info
        private const int VersionByte = 7;
        private const int VersionByteMask = 0x0f;
        private const int VersionByteShift = 4;

        // indexes within the uuid array for certain boundaries
        private const byte TimestampByte = 0;
        private const byte GuidClockSequenceByte = 8;
        private const byte NodeByte = 10;


        public static byte[] NodeBytes { get; set; }
        public static byte[] ClockSequenceBytes { get; set; }

        static GuidGenerator()
        {
            Random = new Random();
            try
            {
                var nic = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up);
                // ReSharper disable once PossibleNullReferenceException
                NodeBytes = GenerateNodeBytes(nic.GetPhysicalAddress());
            }
            catch
            {
                NodeBytes = GenerateNodeBytes();
            }

            _lastClockSequenceForNoDuplicatesGeneration = 0;
            ClockSequenceBytes = GenerateClockSequenceBytes();
        }

        /// <summary>
        /// Generates a random node ID. While not RFC4122 compliant, in practical terms still useful.
        /// This is used as a fallback for when the MAC Address can not be obtained.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateNodeBytes()
        {
            var node = new byte[6];

            Random.NextBytes(node);
            return node;
        }

        /// <summary>
        /// Generates a node ID from the provided IPAddress. While not technically RFC4122 compliant, it may still be useful for scenarios
        /// where exposing the MAC address would be a security concern.
        /// </summary>
        public static byte[] GenerateNodeBytes(IPAddress ip)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));

            var bytes = ip.GetAddressBytes();

            if (bytes.Length < 6)
                throw new ArgumentOutOfRangeException(nameof(ip), "The passed in IP address must contain at least 6 bytes.");

            var node = new byte[6];
            Array.Copy(bytes, node, 6);

            return node;
        }
        
        /// <summary>
        /// Generates a node ID from the provided MAC Address. This is the only RFC4122 compliant node input.
        /// </summary>
        public static byte[] GenerateNodeBytes(PhysicalAddress mac)
        {
            if (mac == null)
                throw new ArgumentNullException(nameof(mac));

            var node = mac.GetAddressBytes();

            return node;
        }

        /// <summary>
        /// Generates a clock sequence. This is used to help avoid duplicates that could arise when
        /// the clock is set backwards in time or if the node ID changes.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateClockSequenceBytes()
        {
            if (_lastClockSequenceForNoDuplicatesGeneration == short.MaxValue)
            {
                _lastClockSequenceForNoDuplicatesGeneration = 0;
            }
            else
            {
                _lastClockSequenceForNoDuplicatesGeneration++;
            }
            var bytes = BitConverter.GetBytes(_lastClockSequenceForNoDuplicatesGeneration);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// Gets a RFC4122 compliant timestamp for Version 1 UUIDs
        /// </summary>
        public static long GetTicksSinceGregorianCalendarEpoch()
        {
            return DateTimeProvider.GetSystemTimePrecise().MinusGregorianCalendarEpoch().Ticks;
        }

        /// <summary>
        /// Generates a RFC4122-compliant Version 1 UUID and returns it as a Guid
        /// </summary>
        public static Guid GenerateTimeBasedGuid()
        {
            lock (Lock)
            {
                var ts = GetTicksSinceGregorianCalendarEpoch();

                if (ts <= _lastTimestampForNoDuplicatesGeneration)
                {
                    ClockSequenceBytes = GenerateClockSequenceBytes();
                }
                else
                {
                    _lastClockSequenceForNoDuplicatesGeneration = 0;
                    ClockSequenceBytes = GenerateClockSequenceBytes();
                }

                _lastTimestampForNoDuplicatesGeneration = ts;

                return GenerateTimeBasedGuid(ts, ClockSequenceBytes, NodeBytes);
            }
        }

        /// <summary>
        /// Generates a Version 1 UUID based on manually provided timestamp, clock sequence and node ID, and returns it as a Guid
        /// </summary>
        public static Guid GenerateTimeBasedGuid(long ticksSinceUnixEpoch, byte[] clockSequence, byte[] node)
        {
            if (clockSequence == null)
                throw new ArgumentNullException(nameof(clockSequence));

            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (clockSequence.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(clockSequence), "The clockSequence must be 2 bytes.");

            if (node.Length != 6)
                throw new ArgumentOutOfRangeException(nameof(node), "The node must be 6 bytes.");

            var guid = new byte[ByteArraySize];
            var timestamp = BitConverter.GetBytes(ticksSinceUnixEpoch);

            // copy node
            Array.Copy(node, 0, guid, NodeByte, Math.Min(6, node.Length));

            // copy clock sequence
            Array.Copy(clockSequence, 0, guid, GuidClockSequenceByte, Math.Min(2, clockSequence.Length));

            // copy timestamp
            Array.Copy(timestamp, 0, guid, TimestampByte, Math.Min(8, timestamp.Length));

            // set the variant
            guid[VariantByte] &= VariantByteMask;
            guid[VariantByte] |= VariantByteShift;

            // set the version
            guid[VersionByte] &= VersionByteMask;
            guid[VersionByte] |= (byte)GuidVersion.TimeBased << VersionByteShift;

            return new Guid(guid);
        }
    }

    public enum GuidVersion
    {
        TimeBased = 0x01,
        Reserved = 0x02,
        NameBased = 0x03,
        Random = 0x04
    }
}
