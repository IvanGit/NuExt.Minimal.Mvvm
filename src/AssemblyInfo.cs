#if NETFRAMEWORK || WINDOWS
using System.Windows.Markup;

[assembly: XmlnsPrefix("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "minimal")]
[assembly: XmlnsDefinition("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "Minimal.Mvvm")]
[assembly: XmlnsDefinition("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "Minimal.Mvvm.UI")]
#endif