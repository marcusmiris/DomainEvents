using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Miris.DomainEvents.Extensions
{
    internal static class EnumerableExtensions
    {
        [DebuggerStepThrough]
        public static void ForEach<T>(
            this IEnumerable<T> source,
            Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            var list = source as List<T>;
            if (list != null)
                list.ForEach(action);
            else
                foreach (var item in source) action(item);
        }
    }
}
