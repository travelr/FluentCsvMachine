using System.Diagnostics.CodeAnalysis;

namespace FluentCsvMachine.Helpers
{
    public static class EnumerablesExtension
    {
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (T item in sequence)
            {
                action(item);
            }
        }

        /// <summary>
        /// ForEach with Index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T, int> action)
        {
            int i = 0;
            foreach (T item in sequence)
            {
                action(item, i);
                i++;
            }
        }

        public static bool IsEmpty<T>([AllowNull] this IEnumerable<T> sequence)
        {
            return sequence == null || !sequence.Any();
        }
    }
}