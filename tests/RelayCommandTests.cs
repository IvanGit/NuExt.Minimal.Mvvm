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
            int executedCount = 0;
            RelayCommand command = null!;
            command = new RelayCommand(Execute) { AllowConcurrentExecution = true };

            (command as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;
           
            for (int i = 0; i < 100; i++)
            {
                executedCount = 0;
                Assert.That(command.IsExecuting, Is.False);
                var tasks = new List<Task>();
                int num = 5;
                for (int j = 0; j < num; j++)
                {
                    tasks.Add(Task.Run(() => ExecuteCommand(null)));
                }
                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
                Assert.Multiple(() =>
                {
                    Assert.That(command.IsExecuting, Is.False);
                    Assert.That(executedCount, Is.EqualTo(num));
                });
            }

            Assert.Pass();

            void Execute()
            {
                Interlocked.Increment(ref executedCount);
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
                }
            }
        }
    }
}