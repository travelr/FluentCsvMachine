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

        public static bool IsEmpty<T>([AllowNull] this IEnumerable<T> sequence)
        {
            return sequence == null || !sequence.Any();
        }
    }
}