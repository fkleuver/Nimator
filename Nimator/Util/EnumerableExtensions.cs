using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimator.Util
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f, Func<T, IEnumerable<T>, IEnumerable<T>> fAssign = null)
        {
            if (fAssign == null)
            {
                return e.SelectMany(c => f(c).Flatten(f)).Concat(e);
            }
            else
            {
                return e.SelectMany(c => fAssign(c, f(c)).Flatten(f, fAssign)).Concat(e);
            }
        }

        public static IEnumerable<T> Flatten<T>(this T e, Func<T, IEnumerable<T>> f, Func<T, IEnumerable<T>, IEnumerable<T>> fAssign = null)
        {
            if (fAssign == null)
            {
                return f(e).Flatten(f).Concat(new[] { e });
            }
            else
            {
                return fAssign(e, f(e)).Flatten(f, fAssign).Concat(new[] { e });
            }
        }
    }
}
