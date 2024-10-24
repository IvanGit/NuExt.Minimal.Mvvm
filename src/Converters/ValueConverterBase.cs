#if NETFRAMEWORK || WINDOWS
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Minimal.Mvvm.UI
{
    [DebuggerStepThrough]
    public abstract class ValueConverterBase<TFrom, TTo, TParameter> : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            Cast<TTo>.ThrowIfNotIsAssignableFrom(targetType);
            var v = Cast<TFrom>.To(value, false);
            var p = Cast<TParameter>.To(parameter, false);
            return ConvertTo(v, p, culture);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            Cast<TFrom>.ThrowIfNotIsAssignableFrom(targetType);
            var v = Cast<TTo>.To(value, false);
            var p = Cast<TParameter>.To(parameter, false);
            return ConvertFrom(v, p, culture);
        }

        protected abstract TTo? ConvertTo(TFrom? value, TParameter? parameter, CultureInfo? culture);

        protected virtual TFrom? ConvertFrom(TTo? value, TParameter? parameter, CultureInfo? culture)
        {
            throw new NotSupportedException();
        }
    }
}
#endif