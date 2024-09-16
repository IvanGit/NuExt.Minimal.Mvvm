using Minimal.Mvvm;
using System.ComponentModel;

using static NUnit.Framework.TestContext;

namespace NuExt.Minimal.Mvvm.Tests
{
    public class RelayCommandTests
    {
        [Test]
        public async Task MultipleExecuteTestAsync()
        {
            RelayCommand command = null!;
            command = new RelayCommand(Execute);

            bool isExecuting = false;
            (command as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;
           
            for (int i = 0; i < 100; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(isExecuting, Is.False);
                    Assert.That(command.IsExecuting, Is.False);
                });
                var tasks = new List<Task>();
                for (int j = 0; j < 5; j++)
                {
                    tasks.Add(Task.Run(() => ExecuteCommand(null)));
                }
                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
                Assert.Multiple(() =>
                {
                    Assert.That(command.IsExecuting, Is.False);
                    Assert.That(isExecuting, Is.False);
                });
            }

            Assert.Pass();

            void Execute()
            {
                Assert.That(command.IsExecuting, Is.True);
                Progress.WriteLine($"[{command.GetType().Name}] Thread={Environment.CurrentManagedThreadId, -2}, ExecutingCount={command.ExecutingCount}");
            }

            void ExecuteCommand(object? obj)
            {
                command.Execute(obj);
            }

            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(IRelayCommand.IsExecuting))
                {
                    Progress.WriteLine($"[{command.GetType().Name}] Thread={Environment.CurrentManagedThreadId, -2}, ExecutingCount={command.ExecutingCount}, IsExecuting={command.IsExecuting}");
                    isExecuting = command.IsExecuting;
                }
            }
        }
    }
}