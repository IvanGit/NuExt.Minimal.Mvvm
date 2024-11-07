#if NETFRAMEWORK || WINDOWS
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// An abstract base class for implementing value converters in WPF, providing a generic way 
    /// to convert values between different types with optional parameters and culture information.
    /// </summary>
    /// <typeparam name="TFrom">The type of the input value.</typeparam>
    /// <typeparam name="TTo">The type of the output value.</typeparam>
    /// <typeparam name="TParameter">The type of the parameter used in conversion.</typeparam>
    [DebuggerStepThrough]
    public abstract class ValueConverterBase<TFrom, TTo, TParameter> : IValueConverter
    {
        /// <summary>
        /// Converts a value from the source type to the target type.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            Cast<TTo>.ThrowIfNotIsAssignableFrom(targetType);
            var v = Cast<TFrom>.To(value, false);
            var p = Cast<TParameter>.To(parameter, false);
            return ConvertTo(v, p, culture);
        }

        /// <summary>
        /// Converts a value from the target type back to the source type.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            Cast<TFrom>.ThrowIfNotIsAssignableFrom(targetType);
            var v = Cast<TTo>.To(value, false);
            var p = Cast<TParameter>.To(parameter, false);
            return ConvertFrom(v, p, culture);
        }

        /// <summary>
        /// When overridden in a derived class, converts a value from the source type to the target type.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        protected abstract TTo? ConvertTo(TFrom? value, TParameter? parameter, CultureInfo? culture);

        /// <summary>
        /// Converts a value from the target type back to the source type.
        /// This implementation throws a <see cref="NotSupportedException"/> by default.
        /// Override this method in a derived class to provide custom backward conversion logic.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        protected virtual TFrom? ConvertFrom(TTo? value, TParameter? parameter, CultureInfo? culture)
        {
            throw new NotSupportedException();
        }
    }
}
#endif