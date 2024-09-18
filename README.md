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

In addition to the standard package, there is also a source code package available: `NuExt.Minimal.Mvvm.Sources`. This package allows you to embed the entire framework directly into your application, enabling easier source code exploring and debugging.

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
        CancelCommand = new RelayCommand(ExecuteCancel, CanCancel);
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

    private void ExecuteCancel()
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

### Contributing

Contributions are welcome! Feel free to submit issues, fork the repository, and send pull requests. Your feedback and suggestions for improvement are highly appreciated.

### License

Licensed under the MIT License. See the LICENSE file for details.