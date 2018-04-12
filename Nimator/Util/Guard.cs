using System;
using System.Collections;
using System.Diagnostics;

namespace Nimator.Util
{
    public static class Guard
    {
        [ContractAnnotation("value: null => halt"), DebuggerStepThrough]
        public static void AgainstNull([InvokerParameterName] string argumentName, [NotNull] object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        [ContractAnnotation("value: null => halt"), DebuggerStepThrough]
        public static T AgainstNull_Return<T>([InvokerParameterName] string argumentName, [NotNull] T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            return value;
        }

        [ContractAnnotation("value: null => halt"), DebuggerStepThrough]
        public static void AgainstNullAndEmpty([InvokerParameterName] string argumentName, [NotNull] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        [ContractAnnotation("value: null => halt"), DebuggerStepThrough]
        public static string AgainstNullAndEmpty_Return([InvokerParameterName] string argumentName, [NotNull] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(argumentName);
            }
            return value;
        }

        [ContractAnnotation("value: null => halt"), DebuggerStepThrough]
        public static void AgainstNullAndEmpty([InvokerParameterName] string argumentName, [NotNull, NoEnumeration] ICollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            if (value.Count == 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        [ContractAnnotation("value: null => halt"), DebuggerStepThrough]
        public static T AgainstNullAndEmpty_Return<T>([InvokerParameterName] string argumentName, [NotNull, NoEnumeration] T value) where T : ICollection
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            if (value.Count == 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
            return value;
        }
        
        [DebuggerStepThrough]
        public static void AgainstNegative([InvokerParameterName] string argumentName, int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }
        
        [DebuggerStepThrough]
        public static int AgainstNegative_Return([InvokerParameterName] string argumentName, int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
            return value;
        }
    }
}
