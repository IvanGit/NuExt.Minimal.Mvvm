# NuExt.Minimal.Mvvm

`NuExt.Minimal.Mvvm` is a lightweight MVVM (Model-View-ViewModel) framework designed to provide the essential components needed for implementing the MVVM pattern in .NET applications. This package focuses on supporting proper concurrent operations and does not have any external dependencies, making it easy to integrate and use in various projects.

### Features

- **Command Implementations**:
  - **`Minimal.Mvvm.RelayCommand`**: A simple command implementation that delegates its execution logic via delegates.
  - **`Minimal.Mvvm.RelayCommand<T>`**: A relay command that operates with generic parameters.

- **Asynchronous Command Support**:
  - **`Minimal.Mvvm.AsyncCommand`**: An asynchronous command that facilitates non-blocking operations without parameters.
  - **`Minimal.Mvvm.AsyncCommand<T>`**: An asynchronous command capable of handling operations with generic parameters.

- **Simplified Model Development**:
  - **`Minimal.Mvvm.BindableBase`**: A base class that implements `INotifyPropertyChanged`, simplifying property change notification in models.

- **ViewModels Development**:
  - **`Minimal.Mvvm.ViewModelBase`**: A base class for ViewModels, providing a foundation for building complex view models.

- **Concurrent Command Execution**:
  - All `IRelayCommand` command types (`RelayCommand`, `RelayCommand<T>`, `AsyncCommand`, `AsyncCommand<T>`) support concurrent executions. This allows multiple invocations of the same command simultaneously without interfering with other executions.

- **CompositeCommand Implementation**:
  - **`Minimal.Mvvm.CompositeCommand`**: Represents a command that aggregates multiple commands and executes them sequentially. This is useful when you need to perform a series of actions as a single command operation.

- **Service Provider Integration**:
  - **`Minimal.Mvvm.ServiceProvider`**: A service provider class that allows for easy registration and resolution of services, facilitating dependency injection within your application.

- **Interactivity Support**:
  - **`Minimal.Mvvm.Windows.EventTrigger`**: Executes a command in response to an event, with support for converting event arguments before passing them to the command or passing the event arguments directly.
  - **`Minimal.Mvvm.Windows.KeyTrigger`**: Executes a command in response to a specific key gesture, with default association to the UIElement's KeyUp event.
  - **`Minimal.Mvvm.Windows.WindowService`**: Provides a service for interacting with a Window associated with a FrameworkElement.

### Recommended Companion Package

For an enhanced development experience, we highly recommend using the [`NuExt.Minimal.Mvvm.SourceGenerator`](https://www.nuget.org/packages/NuExt.Minimal.Mvvm.SourceGenerator) package alongside this framework. It provides a source generator that produces boilerplate code for your ViewModels at compile time, significantly reducing the amount of repetitive coding tasks and allowing you to focus more on the application-specific logic.

### Installation

You can install `NuExt.Minimal.Mvvm` via [NuGet](https://www.nuget.org/):

```sh
dotnet add package NuExt.Minimal.Mvvm
```

Or through the Visual Studio package manager:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.Minimal.Mvvm`.
3. Click "Install".

### Source Code Package

In addition to the standard package, there is also a source code package available: [`NuExt.Minimal.Mvvm.Sources`](https://www.nuget.org/packages/NuExt.Minimal.Mvvm.Sources). This package allows you to embed the entire framework directly into your application, enabling easier source code exploring and debugging.

To install the source code package, use the following command:

```sh
dotnet add package NuExt.Minimal.Mvvm.Sources
```

Or through the Visual Studio package manager:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.Minimal.Mvvm.Sources`.
3. Click "Install".

### Usage

#### Example Using AsyncCommand with CancellationTokenSource Support

```csharp
public class MyViewModel : ViewModelBase
{
    public IAsyncCommand MyCommand { get; }
    public ICommand CancelCommand { get; }

    public MyViewModel()
    {
        MyCommand = new AsyncCommand(ExecuteAsync, CanExecute);
        CancelCommand = new RelayCommand(Cancel, CanCancel);
    }

    private async Task ExecuteAsync()
    {
        // Retrieve the CancellationTokenSource for current execution method instance
        var cts = MyCommand.CancellationTokenSource;
        try
        {
            // Command execution logic
            await Task.Delay(1000, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
        }
    }

    private bool CanExecute()
    {
        // Logic that determines whether the command can execute
        return true;
    }

    private void Cancel()
    {
        // Sends request to cancel the MyCommand execution
        MyCommand.Cancel();
    }

    private bool CanCancel()
    {
        // Logic that determines whether the cancel command can execute
        return MyCommand.IsExecuting;
    }
}
```

#### Example Using BindableBase

```csharp
public class MyModel : BindableBase
{
    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

#### Example Using ServiceProvider

```csharp
public class MyService
{
    public string GetData() => "Hello from MyService!";
}

public class MyViewModel : ViewModelBase
{
    public IRelayCommand MyCommand { get; }

    public MyViewModel()
    {
        // Register services
        ServiceProvider.Default.RegisterService<MyService>();
        
        MyCommand = new RelayCommand(() =>
        {
            // Resolve and use services
            var myService = ServiceProvider.Default.GetService<MyService>();

            var data = myService.GetData();
            // Use the data
        });
    }
}
```

#### Example Using Interactivity in XAML

You can use `EventTrigger` and `KeyTrigger` in your XAML to bind events and key gestures to commands in your ViewModel. Here's an example of a window that binds `ContentRendered`, `Loaded` events, and a `CTRL+O` key gesture to commands in the ViewModel.

##### ViewModel

```csharp
public class MyViewModel : ViewModelBase
{
    public ICommand CloseCommand { get; }
    public ICommand ContentRenderedCommand { get; }
    public ICommand LoadedCommand { get; }
    public ICommand OpenFileCommand { get; }
    public WindowService? WindowService => GetService<WindowService>();

    public MyViewModel()
    {
        CloseCommand = new RelayCommand(Close, CanClose);
        ContentRenderedCommand = new RelayCommand(OnContentRendered);
        LoadedCommand = new RelayCommand(OnLoaded);
        OpenFileCommand = new RelayCommand(OnOpenFile);
    }

    private bool CanClose()
    {
        return true;
    }

    private void Close()
    {
        WindowService?.Close();//Closes the window.
    }

    private void OnContentRendered()
    {
        // Logic when content is rendered
    }

    private void OnLoaded()
    {
        // Logic when window is loaded
    }

    private void OnOpenFile()
    {
        // Logic to open a file
    }
}
```

##### XAML

```xml
<Window x:Class="YourNamespace.MyWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:minimal="clr-namespace:Minimal.Mvvm.Windows;assembly=NuExt.Minimal.Mvvm"
        Title="My Window">
    <Window.DataContext>
        <local:MyViewModel />
    </Window.DataContext>
    <minimal:Interaction.Behaviors>
        <minimal:WindowService/>
        <minimal:EventTrigger EventName="ContentRendered" Command="{Binding ContentRenderedCommand}"/>
        <minimal:EventTrigger EventName="Loaded" Command="{Binding LoadedCommand}" />
        <minimal:KeyTrigger Gesture="CTRL+O" Command="{Binding OpenFileCommand}" />
        <minimal:KeyTrigger Gesture="CTRL+X" Command="{Binding CloseCommand}" />
    </minimal:Interaction.Behaviors>
    <Grid>
        <!-- Your UI elements here -->
    </Grid>
</Window>
```

#### Example Usage with Source Generator

To further simplify your ViewModel development, consider using the source generator provided by the `NuExt.Minimal.Mvvm.SourceGenerator` package. Here's an example:

```csharp
using Minimal.Mvvm;

public partial class MyModel : BindableBase
{
    [Notify]
    private string? _description;

    [Notify(Setter = AccessModifier.Private)]
    private string _name;
}
```

The source generator would produce the following code:

```csharp
partial class MyModel
{
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Name
    {
        get => _name;
        private set => SetProperty(ref _name, value);
    }
}
```

This automation helps to maintain clean and efficient code, improving overall productivity. For details on installing and using the source generator, refer to the [NuExt.Minimal.Mvvm.SourceGenerator](https://github.com/IvanGit/NuExt.Minimal.Mvvm.SourceGenerator) documentation.

### Contributing

Contributions are welcome! Feel free to submit issues, fork the repository, and send pull requests. Your feedback and suggestions for improvement are highly appreciated.

### License

Licensed under the MIT License. See the LICENSE file for details.