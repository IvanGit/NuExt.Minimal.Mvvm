#if NETFRAMEWORK || WINDOWS
using System.Globalization;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Converts a boolean value to a <see cref="Visibility"/> enumeration value and vice versa.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : ValueConverterBase<bool, Visibility, object>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the converter should return 
        /// <see cref="Visibility.Hidden"/> instead of <see cref="Visibility.Collapsed"/>
        /// when the boolean value is false.
        /// </summary>
        public bool HideInsteadOfCollapse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the conversion logic should be inverted.
        /// If true, the visibility will be reversed (i.e., true becomes Collapsed/Hidden and false becomes Visible).
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// Converts a boolean value to a <see cref="Visibility"/> value.
        /// </summary>
        /// <param name="value">The boolean value produced by the binding source.</param>
        /// <param name="parameter">An optional parameter to use in the converter. Not used in this implementation.</param>
        /// <param name="culture">The culture to use in the converter. Not used in this implementation.</param>
        /// <returns>A <see cref="Visibility"/> value based on the boolean input value and the current settings of <see cref="HideInsteadOfCollapse"/> and <see cref="Invert"/>.</returns>
        protected override Visibility ConvertTo(bool value, object? parameter, CultureInfo? culture)
        {
            return (value ^ Invert) ? Visibility.Visible : HideInsteadOfCollapse ? Visibility.Hidden : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a <see cref="Visibility"/> value back to a boolean value.
        /// </summary>
        /// <param name="value">The <see cref="Visibility"/> value that is produced by the binding target.</param>
        /// <param name="parameter">An optional parameter to use in the converter. Not used in this implementation.</param>
        /// <param name="culture">The culture to use in the converter. Not used in this implementation.</param>
        /// <returns>A boolean value based on the <see cref="Visibility"/> input value and the current setting of <see cref="Invert"/>.</returns>
        protected override bool ConvertFrom(Visibility value, object? parameter, CultureInfo? culture)
        {
            return (value == Visibility.Visible) ^ Invert;
        }
    }
}
#endif