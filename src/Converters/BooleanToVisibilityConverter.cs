#if NETFRAMEWORK || WINDOWS
using System.Globalization;
using System.Windows;

namespace Minimal.Mvvm
{
    public sealed class BooleanToVisibilityConverter : ValueConverterBase<bool, Visibility, bool?>
    {
        public bool Invert { get; set; }

        protected override Visibility ConvertTo(bool value, bool? parameter, CultureInfo? culture)
        {
            if (!(value ^ Invert))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
    }
}
#endif