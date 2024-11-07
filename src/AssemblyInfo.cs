#if NETFRAMEWORK || WINDOWS
using System.Windows.Markup;

[assembly: XmlnsPrefix("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "minimal")]
[assembly: XmlnsDefinition("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "Minimal.Mvvm")]
[assembly: XmlnsDefinition("http://schemas.minimalmvvm.com/winfx/xaml/mvvm", "Minimal.Mvvm.Windows")]

#if NETFRAMEWORK
namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>Initializes the attribute with the specified return value condition.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    /// <summary>Gets the return value condition.</summary>
    public bool ReturnValue { get; }
}
#endif
#endif