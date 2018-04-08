using System;
using Nimator.Util;

namespace Nimator.CouchBase.Util
{
    public static class TypeExtensions
    {
        public static bool IsIResult([NotNull]this object value)
        {
            Type type = null;
            return value.IsIResult(ref type);
        }

        public static bool IsIResult([NotNull]this object value, ref Type genericTypeArg)
        {
            Guard.AgainstNull(nameof(value), value);

            if (value is IDataCollectionResult result && result.Data != null)
            {
                var dataType = result.Data.GetType();
                if (dataType.IsGenericType)
                {
                    var genericDataType = dataType.GetGenericTypeDefinition();
                    if (genericDataType.IsClass)
                    {
                        genericDataType = genericDataType.GetInterface("IResult`1");
                    }

                    if (genericDataType != null && genericDataType.Name == "IResult`1") // assembly type mismatch issue, need to fix this
                    {
                        genericTypeArg = dataType.GetGenericArguments()[0];
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
