using FluentCsvMachine.Exceptions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FluentCsvMachine.Helpers
{
    /// <summary>
    /// Helper methods to efficiently throw exceptions.
    /// Source partially:
    /// https://github.com/CommunityToolkit/dotnet/blob/0e150d58f3662a0f30e70ee3eee513f41196b3ca/CommunityToolkit.Diagnostics/Internals/Guard.ThrowHelper.cs
    /// </summary>
    [StackTraceHidden]
    internal static class ThrowHelper
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> when <see cref="IsNotNull{T}(T,string)"/> fails.
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        [DoesNotReturn]
        public static void ThrowArgumentNullExceptionForIsNotNull<T>(string name)
        {
            throw new ArgumentNullException(name, $"Parameter {AssertString(name)} ({typeof(T).ToString()}) must be not null).");
        }

        /// <summary>
        /// Setup for parsing the CSV file is not correct
        /// </summary>
        /// <param name="message">Concrete issue</param>
        [DoesNotReturn]
        public static void ThrowCsvConfigurationException(string message)
        {
            throw new CsvConfigurationException(message);
        }

        /// <summary>
        /// CSV cannot be parsed
        /// </summary>
        /// <param name="message">Concrete issue</param>
        [DoesNotReturn]
        public static void ThrowCsvMalformedException(string message)
        {
            throw new CsvMalformedException(message);
        }

        /// <summary>
        /// CSV file not found
        /// </summary>
        /// <param name="path">Submitted path to the CSV file</param>
        [DoesNotReturn]
        internal static void ThrowFileNotFoundException(string path)
        {
            throw new FileNotFoundException("Please check the path to your CSV file!", path);
        }

        /// <summary>
        /// Returns a formatted representation of the input value.
        /// </summary>
        /// <param name="obj">The input <see cref="object"/> to format.</param>
        /// <returns>A formatted representation of <paramref name="obj"/> to display in error messages.</returns>
        private static string AssertString(object? obj)
        {
            return obj switch
            {
                string _ => $"\"{obj}\"",
                null => "null",
                _ => $"<{obj}>"
            };
        }
    }
}