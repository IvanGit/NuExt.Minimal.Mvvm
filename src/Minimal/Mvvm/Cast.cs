using System;
using System.Globalization;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Provides a set of static methods for converting objects to the specified generic type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparamref name="T">The target type to which the value should be converted.</typeparamref>
    public static class Cast<T>
    {
        private static readonly Type s_genericType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        private static readonly bool s_isEnum = s_genericType.IsEnum;
        private static readonly bool s_isValueType = s_genericType.IsValueType;

        public static void ThrowIfNotIsAssignableFrom(Type targetType)
        {
            if (!targetType.IsAssignableFrom(s_genericType))
            {
                throw new InvalidCastException();
            }
        }

        /// <summary>
        /// Converts the given value to the specified generic type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="value">The value to be converted. Can be null.</param>
        /// <param name="throwCastException">
        /// If true, an exception will be thrown if the conversion fails; otherwise, default value of type <typeparamref name="T"/> will be returned.
        /// </param>
        /// <returns>
        /// The converted value of type <typeparamref name="T"/>, or default value of type <typeparamref name="T"/> if the conversion fails and <paramref name="throwCastException"/> is false.
        /// </returns>
        /// <exception cref="InvalidCastException">Thrown when the conversion to <typeparamref name="T"/> fails and <paramref name="throwCastException"/> is true.</exception>
        public static T? To(object? value, bool throwCastException = true)
        {
            switch (value)
            {
                case null:
                    return (s_isValueType && !throwCastException) ? default : (T?)value;
                case T @param:
                    return @param;
                default:
                    try
                    {
                        if (s_isEnum)
                        {
                            return value switch
                            {
                                string s => (T?)Enum.Parse(s_genericType, s, ignoreCase: false),
                                _ => (T?)Enum.ToObject(s_genericType, value)
                            };
                        }
                        if (value is IConvertible)
                        {
                            return (T?)Convert.ChangeType(value, s_genericType, CultureInfo.InvariantCulture);
                        }

                        if (!throwCastException) return default;
                        return (T?)value;
                    }
                    catch when (!throwCastException)
                    {
                        return default;
                    }
            }
        }
    }
}
