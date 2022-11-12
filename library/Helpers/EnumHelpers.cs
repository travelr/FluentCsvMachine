using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCsvMachine.Helpers
{
    /// <summary>
    /// https://stackoverflow.com/a/34775560
    /// </summary>
    /// <typeparam name="TTarget">Enum Type</typeparam>
    public static class EnumHelpers<TTarget>
    {
        private static Dictionary<string, TTarget> Dict => Enum.GetNames(typeof(TTarget))
                                                               .ToDictionary(x => x, x => (TTarget)Enum.Parse(typeof(TTarget), x), StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Convert string to Enum
        /// </summary>
        /// <param name="value">string</param>
        /// <returns>Enum value</returns>
        public static TTarget Convert(string value)
        {
            return Dict[value];
        }
    }
}