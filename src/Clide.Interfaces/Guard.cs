using System;
using System.Diagnostics;

namespace Clide
{
    /// <summary>
    /// Common guard class for argument validation.
    /// </summary>
    [DebuggerStepThrough]
    public static partial class Guard
    {
        /// <summary>
        /// Ensures the given <paramref name="value"/> is not null.
        /// Throws <see cref="ArgumentNullException"/> otherwise.
        /// </summary>
        /// <exception cref="System.ArgumentException">The <paramref name="value"/> is null.</exception>
        public static void NotNull<T>(string name, T value)
        {
            if (value == null)
                throw new ArgumentNullException(name, "Parameter cannot be null.");
        }

        /// <summary>
        /// Ensures the given string <paramref name="value"/> is not null or empty.
        /// Throws <see cref="ArgumentNullException"/> in the first case, or 
        /// <see cref="ArgumentException"/> in the latter.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="value"/> is null or an empty string.</exception>
        public static void NotNullOrEmpty(string name, string value)
        {
            NotNull(name, value);
            if (value.Length == 0)
                throw new ArgumentException("Parameter cannot be empty.", name);
        }

        /// <summary>
        /// Ensures the given string <paramref name="value"/> is valid according 
        /// to the <paramref name="validate"/> function. Throws <see cref="ArgumentException"/> 
        /// otherwise, with the given <paramref name="message"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="value"/> is not valid according 
        /// to the <paramref name="validate"/> function.</exception>
        public static void IsValid<T>(string name, T value, Func<T, bool> validate, string message)
        {
            if (!validate(value))
                throw new ArgumentException(message, name);
        }

        /// <summary>
        /// Ensures the given string <paramref name="value"/> is valid according 
        /// to the <paramref name="validate"/> function. Throws <see cref="ArgumentException"/> 
        /// otherwise, with a message built by applying the given <paramref name="format"/> and 
        /// <paramref name="args"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="value"/> is not valid according 
        /// to the <paramref name="validate"/> function.</exception>
        public static void IsValid<T>(string name, T value, Func<T, bool> validate, string format, params object[] args)
        {
            if (!validate(value))
                throw new ArgumentException(string.Format(format, args), name);
        }
    }
}