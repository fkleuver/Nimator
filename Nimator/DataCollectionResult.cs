using System;
using System.Linq;
using Nimator.Util;

namespace Nimator
{
    /// <inheritdoc cref="IDataCollectionResult" />
    public class DataCollectionResult : IDataCollectionResult, IConvertible
    {
        /// <inheritdoc />
        public IDataCollector Origin { get; }

        /// <inheritdoc />
        public long Start { get; }

        /// <inheritdoc />
        public long End { get; }

        /// <inheritdoc />
        public Exception Error { get; }

        /// <inheritdoc />
        public object Data { get; }
        
        /// <summary>
        /// Whether the operation succeeded or not.
        /// </summary>
        public bool Success => Error == null;

        public DataCollectionResult([NotNull]IDataCollector origin, long start, long end, [NotNull]Exception data) : this(origin, start, end, (object)data) { }
        public DataCollectionResult([NotNull]IDataCollector origin, long start, long end, [NotNull]object data)
        {
            Guard.AgainstNull(nameof(origin), origin);
            Guard.AgainstNull(nameof(data), data);

            Origin = origin;
            Start = start;
            End = end;
            if (data is Exception ex)
            {
                Error = ex;
            }
            else
            {
                Data = data;
            }
        }

        #region IConvertible implementation
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            // This is about as ugly as it gets; need to find a MUCH cleaner and refactor-proof way of casting from non-generic to generic
            var typeArg = conversionType.GetGenericArguments()[0];
            var ctor = conversionType.GetConstructors().FirstOrDefault(c => c.GetParameters()[3].ParameterType == (Data != null ? typeArg : typeof(Exception)));
            if (ctor != null)
            {
                return ctor.Invoke(new[] { Origin, Start, End, Data ?? Error });
            }
            throw new NotImplementedException();
        }
        #endregion
    }

    /// <inheritdoc />
    public class DataCollectionResult<TData> : DataCollectionResult where TData : class
    {
        public DataCollectionResult([NotNull]IDataCollector origin, long start, long end, [NotNull]TData data) : base(origin, start, end, data) { }
        public DataCollectionResult([NotNull]IDataCollector origin, long start, long end, [NotNull]Exception data) : base(origin, start, end, data) { }
        public new TData Data => ((DataCollectionResult)this).Data as TData;
    }
}
