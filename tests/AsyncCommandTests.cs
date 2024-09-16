using Minimal.Mvvm;
using System.ComponentModel;
using static NUnit.Framework.TestContext;

namespace NuExt.Minimal.Mvvm.Tests
{
    internal class AsyncCommandTests
    {
        [Test]
        public async Task MultipleExecuteTestAsync()
        {
            int executedCount = 0;
            AsyncCommand command = null!;
            command = new AsyncCommand(ExecuteAsync);

            (command as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;

            for (int i = 0; i < 100; i++)
            {
                executedCount = 0;
                for (int j = 0; j < 5; j++)
                {
                    _ = Task.Run(() => command.Execute(null));
                }
                while (Interlocked.CompareExchange(ref executedCount, 0, 0) != 5)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                }
                while (!command.ExecutingTasks.IsEmpty)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                }
                await command.WaitAsync();
                Assert.Multiple(() =>
                {
                    Assert.That(command.IsExecuting, Is.False);
                    Assert.That(executedCount, Is.EqualTo(5));
                });
            }

            Assert.Pass();

            async Task ExecuteAsync()
            {
                Interlocked.Increment(ref executedCount);
                var cts = command.CancellationTokenSource!;
                Assert.That(cts, Is.Not.Null);
                Assert.That(command.ExecutingTasks.ContainsKey(cts), Is.True);
                Assert.That(command.ExecutingTasks[cts], Is.EqualTo(Environment.CurrentManagedThreadId));
                Assert.That(command.IsExecuting, Is.True);
                await Progress.WriteLineAsync($"[{command.GetType().Name}] Thread={Environment.CurrentManagedThreadId,-2}, ExecutingCount={command.ExecutingCount}");
                await Task.Yield();
                Assert.That(command.CancellationTokenSource, Is.EqualTo(cts));
            }

            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(IRelayCommand.IsExecuting))
                {
                    Progress.WriteLine($"[{command.GetType().Name}] Thread={Environment.CurrentManagedThreadId,-2}, ExecutingCount={command.ExecutingCount}, IsExecuting={command.IsExecuting}");
                }
            }
        }

        [Test]
        public async Task CancelMultipleExecuteTestAsync()
        {
            int executedCount = 0;
            AsyncCommand command = null!;
            command = new AsyncCommand(ExecuteAsync);

            (command as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;

            for (int i = 0; i < 100; i++)
            {
                await Progress.WriteLineAsync($"[{command.GetType().Name}, {i}] Thread={Environment.CurrentManagedThreadId,-2}, ExecutingCount={command.ExecutingCount}, IsExecuting={command.IsExecuting}");
                Assert.Multiple(() =>
                {
                    Assert.That(command.IsCancellationRequested, Is.False);
                    Assert.That(executedCount, Is.Zero);
                    Assert.That(command.ExecutingCount, Is.EqualTo(0));
                    Assert.That(command.IsExecuting, Is.False);
                });
                for (int j = 0; j < 5; j++)
                {
                    _ = Task.Run(() => command.Execute(null));
                }
                while (Interlocked.CompareExchange(ref executedCount, 0, 0) != 5)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                }
                Assert.Multiple(() =>
                {
                    Assert.That(executedCount, Is.EqualTo(5));
                    Assert.That(command.ExecutingCount, Is.EqualTo(5));
                    Assert.That(command.IsExecuting, Is.True);
                });
                command.Cancel();
                Assert.That(command.IsCancellationRequested, Is.True);
                await command.WaitAsync();
                command.ResetCancel();
                Assert.Multiple(() =>
                {
                    Assert.That(command.IsCancellationRequested, Is.False);
                    Assert.That(executedCount, Is.Zero);
                    Assert.That(command.ExecutingCount, Is.EqualTo(0));
                    Assert.That(command.IsExecuting, Is.False);
                });
            }

            Assert.Pass();

            async Task ExecuteAsync()
            {
                Interlocked.Increment(ref executedCount);
                await Task.Yield();
                var cts = command.CancellationTokenSource!;
                Assert.That(cts, Is.Not.Null);
                try
                {
                    cts.Token.ThrowIfCancellationRequested();
                    for (int i = 0; i < 100; i++)
                    {
                        Assert.That(i, Is.LessThan(50));
                        cts.Token.ThrowIfCancellationRequested();
                        await Task.Delay(100, cts.Token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref executedCount);
                }
            }

            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(IRelayCommand.IsExecuting))
                {
                    Progress.WriteLine($"[{command.GetType().Name}] Thread={Environment.CurrentManagedThreadId,-2}, ExecutingCount={command.ExecutingCount}, IsExecuting={command.IsExecuting}, IsCancellationRequested={command.IsCancellationRequested}");
                }
            }
        }

        [Test]
        public async Task CancelMultipleExecuteAsyncTestAsync()
        {
            int executedCount = 0;
            AsyncCommand command = null!;
            command = new AsyncCommand(ExecuteAsync);

            (command as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;

            for (int i = 0; i < 100; i++)
            {
                await Progress.WriteLineAsync($"[{command.GetType().Name}, {i}] Thread={Environment.CurrentManagedThreadId,-2}, ExecutingCount={command.ExecutingCount}, IsExecuting={command.IsExecuting}");
                Assert.Multiple(() =>
                {
                    Assert.That(command.IsCancellationRequested, Is.False);
                    Assert.That(executedCount, Is.Zero);
                    Assert.That(command.ExecutingCount, Is.EqualTo(0));
                    Assert.That(command.IsExecuting, Is.False);
                });
                for (int j = 0; j < 5; j++)
                {
                    _ = Task.Run(async () => await command.ExecuteAsync(null));
                }
                while (Interlocked.CompareExchange(ref executedCount, 0, 0) != 5)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                }
                Assert.Multiple(() =>
                {
                    Assert.That(executedCount, Is.EqualTo(5));
                    Assert.That(command.ExecutingCount, Is.EqualTo(5));
                    Assert.That(command.IsExecuting, Is.True);
                });
                command.Cancel();
                Assert.That(command.IsCancellationRequested, Is.True);
                await command.WaitAsync();
                command.ResetCancel();
                Assert.Multiple(() =>
                {
                    Assert.That(command.IsCancellationRequested, Is.False);
                    Assert.That(executedCount, Is.Zero);
                    Assert.That(command.ExecutingCount, Is.EqualTo(0));
                    Assert.That(command.IsExecuting, Is.False);
                });
            }

            Assert.Pass();

            async Task ExecuteAsync()
            {
                Interlocked.Increment(ref executedCount);
                await Task.Yield();
                var cts = command.CancellationTokenSource!;
                Assert.That(cts, Is.Not.Null);
                try
                {
                    cts.Token.ThrowIfCancellationRequested();
                    for (int i = 0; i < 100; i++)
                    {
                        Assert.That(i, Is.LessThan(50));
                        cts.Token.ThrowIfCancellationRequested();
                        await Task.Delay(100, cts.Token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref executedCount);
                }
            }

            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(IRelayCommand.IsExecuting))
                {
                    Progress.WriteLine($"[{command.GetType().Name}] Thread={Environment.CurrentManagedThreadId,-2}, ExecutingCount={command.ExecutingCount}, IsExecuting={command.IsExecuting}, IsCancellationRequested={command.IsCancellationRequested}");
                }
            }
        }
    }
}
