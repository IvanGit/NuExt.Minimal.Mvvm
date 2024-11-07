#if NETFRAMEWORK || WINDOWS
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    public class ItemsControlMouseEventArgsConverter : ValueConverterBase<MouseEventArgs, object, Control>
    {
        public ItemsControlMouseEventArgsConverter()
        {

        }
        protected override object? ConvertTo(MouseEventArgs? args, Control? sender, CultureInfo? culture)
        {
            Debug.Assert(sender is TreeViewItem);
            return sender?.DataContext;
        }
    }
}
#endif
