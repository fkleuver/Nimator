using System;
using System.Linq.Expressions;
using System.Text;

namespace Nimator.Util
{
    public static class TypeExcentions
    {
        public static string GetClosedGenericTypeName([NotNull]this Type t)
        {
            Guard.AgainstNull(nameof(t), t);
            if (!t.IsGenericType)
            {
                return t.Name;
            }

            var sb = new StringBuilder();
            sb.Append(t.Name.Substring(0, t.Name.IndexOf('`')));
            sb.Append('<');
            var appendComma = false;
            foreach (var arg in t.GetGenericArguments())
            {
                if (appendComma)
                {
                    sb.Append(',');
                }
                sb.Append(GetClosedGenericTypeName(arg));
                appendComma = true;
            }
            sb.Append('>');
            return sb.ToString();
        }

        public static object Cast([NotNull]this Type type, [NotNull]object value)
        {
            Guard.AgainstNull(nameof(type), type);
            Guard.AgainstNull(nameof(value), value);
            var parameterExpr = Expression.Parameter(typeof(object), "value");
            var blockExpr = Expression.Block(Expression.Convert(Expression.Convert(parameterExpr, value.GetType()), type));

            var lambaExpr = Expression.Lambda(blockExpr, parameterExpr).Compile();
            var result = lambaExpr.DynamicInvoke(value);
            return result;
        }
    }
}
