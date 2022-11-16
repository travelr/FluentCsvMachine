using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCsvMachine.Helpers
{
    /// <summary>
    /// https://github.com/CommunityToolkit/dotnet/blob/4427b4429035e9a8d2f8ee126821b779f92ae7fc/CommunityToolkit.Diagnostics/Guard.cs
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Asserts that the input value is not <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">The type of reference value type being tested.</typeparam>
        /// <param name="value">The input value to test.</param>
        /// <param name="name">The name of the input parameter being tested.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotNull<T>([NotNull] T? value, [CallerArgumentExpression("value")] string name = "")
            where T : class
        {
            if (value is not null)
            {
                return;
            }

            ThrowHelper.ThrowArgumentNullExceptionForIsNotNull<T>(name);
        }

        /// <summary>
        /// Asserts that the input <see cref="string"/> instance must be <see langword="null"/> or empty.
        /// </summary>
        /// <param name="text">The input <see cref="string"/> instance to test.</param>
        /// <param name="name">The name of the input parameter being tested.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="text"/> is neither <see langword="null"/> nor empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNullOrEmpty(string? text, [CallerArgumentExpression("text")] string name = "")
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            ThrowHelper.ThrowArgumentExceptionForIsNullOrEmpty(text, name);
        }

        /// <summary>
        /// Asserts that the input <see cref="string"/> instance must not be <see langword="null"/> or empty.
        /// </summary>
        /// <param name="text">The input <see cref="string"/> instance to test.</param>
        /// <param name="name">The name of the input parameter being tested.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="text"/> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotNullOrEmpty([NotNull] string? text, [CallerArgumentExpression("text")] string name = "")
        {
            if (!string.IsNullOrEmpty(text))
            {
                return;
            }

            ThrowHelper.ThrowArgumentExceptionForIsNotNullOrEmpty(text, name);
        }
    }
}