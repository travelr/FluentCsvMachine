﻿using FluentCsvMachine.Helpers;

namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// CSV cannot be parsed
    /// </summary>
    public class CsvMalformedException : Exception
    {
        public CsvMalformedException(string? message) : base(message)
        {
            Guard.IsNotNull(message);
        }
    }
}