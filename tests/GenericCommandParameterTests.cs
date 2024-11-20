using Minimal.Mvvm;
using System.ComponentModel;

namespace NuExt.Minimal.Mvvm.Tests
{
    class GenericCommandParameterTests
    {
        protected static ICommand<T> CreateCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            return new RelayCommand<T>(execute, canExecute);
        }

        [Test]
        public void GenericEnumTypeCommandTest()
        {
            var command = CreateCommand<BindingDirection>(x => { }, x => true);

            command.CanExecute(BindingDirection.TwoWay);
            command.Execute(BindingDirection.TwoWay);

            command.CanExecute((object)BindingDirection.TwoWay);
            command.Execute((object)BindingDirection.TwoWay);

            command.CanExecute(1);
            command.Execute(1);

            command.CanExecute("TwoWay");
            command.Execute("TwoWay");

            command.CanExecute("x");
            Assert.Throws<ArgumentException>(() => command.Execute("x"));

            command.CanExecute((object?)null);
            Assert.Throws<NullReferenceException>(() => command.Execute((object?)null));

            command.CanExecute(new object());
            Assert.Throws<ArgumentException>(() => command.Execute(new object()));

            command.CanExecute(int.MaxValue);
            command.Execute(int.MaxValue);

            command.CanExecute(long.MaxValue);
            command.Execute(long.MaxValue);

            Assert.Pass();
        }
    }
}
