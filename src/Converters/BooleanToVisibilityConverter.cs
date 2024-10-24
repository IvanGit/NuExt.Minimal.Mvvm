#if NETFRAMEWORK || WINDOWS
using System.Globalization;
using System.Windows;

namespace Minimal.Mvvm.UI
{
    public sealed class BooleanToVisibilityConverter : ValueConverterBase<bool, Visibility, object?>
    {
        public bool HideInsteadOfCollapse { get; set; }

        public bool Invert { get; set; }

        protected override Visibility ConvertTo(bool value, object? parameter, CultureInfo? culture)
        {
            return (value ^ Invert) ? Visibility.Visible : HideInsteadOfCollapse ? Visibility.Hidden : Visibility.Collapsed;
        }

        protected override bool ConvertFrom(Visibility value, object? parameter, CultureInfo? culture)
        {
            return (value == Visibility.Visible) ^ Invert;
        }
    }
}
#endif