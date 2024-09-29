using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minimal.Mvvm;

namespace NuExt.Minimal.Mvvm.Tests
{
    internal class ServiceProviderTests
    {

        [Test]
        public void ServiceContainerTest()
        {
            IServiceContainer provider = new ServiceProvider();

            provider.RegisterService(new StubService("A"));
            provider.RegisterService<IStubService>(new StubService("B"));

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<StubService>, Is.Not.Null);
                Assert.That(provider.GetService<StubService>()!.Name, Is.EqualTo("A"));
                Assert.That(provider.GetService<IStubService>, Is.Not.Null);
                Assert.That(provider.GetService<IStubService>()!.Name, Is.EqualTo("B"));
                Assert.That(provider.GetServices(typeof(StubService)), Is.Not.Null);
                Assert.That(provider.GetServices(typeof(StubService)).Count(), Is.EqualTo(2));
                Assert.That(provider.GetServices(typeof(IStubService)), Is.Not.Null);
                Assert.That(provider.GetServices(typeof(IStubService)).Count(), Is.EqualTo(2));
            });

            provider.RegisterService(new AnotherStubService("C"));
            provider.RegisterService<IStubService>(new AnotherStubService("D"));
            AssertMultiple();
            Cleanup();

            provider.RegisterService(typeof(AnotherStubService), () => new AnotherStubService("C"));
            provider.RegisterService<IStubService>(() => new AnotherStubService("D"));
            AssertMultiple();
            Cleanup();

            provider.RegisterService<AnotherStubService>();
            provider.RegisterService<IStubService>(() => new AnotherStubService("D"));
            AssertMultiple();
            Cleanup();

            provider.RegisterService(typeof(AnotherStubService));
            provider.RegisterService<IStubService>(() => new AnotherStubService("D"));
            AssertMultiple();
            Cleanup();

            Assert.Pass();

            void AssertMultiple()
            {
                Assert.Multiple(() =>
                {
                    Assert.That(provider.GetService<StubService>, Is.Not.Null);
                    Assert.That(provider.GetService<StubService>()!.Name, Is.EqualTo("A"));
                    Assert.That(provider.GetService<AnotherStubService>, Is.Not.Null);
                    Assert.That(provider.GetService<AnotherStubService>()!.Name, Is.EqualTo("C"));
                    Assert.That(provider.GetService<IStubService>, Is.Not.Null);
                    Assert.That(provider.GetService<IStubService>()!.Name, Is.EqualTo("D"));
                    Assert.That(provider.GetServices(typeof(StubService)), Is.Not.Null);
                    Assert.That(provider.GetServices(typeof(StubService)).Count(), Is.EqualTo(1));
                    Assert.That(provider.GetServices(typeof(AnotherStubService)), Is.Not.Null);
                    Assert.That(provider.GetServices(typeof(AnotherStubService)).Count(), Is.EqualTo(2));
                    Assert.That(provider.GetServices(typeof(IStubService)), Is.Not.Null);
                    Assert.That(provider.GetServices(typeof(IStubService)).Count(), Is.EqualTo(3));
                });
            }

            void Cleanup()
            {
                provider.UnregisterService<AnotherStubService>();
                Assert.That(provider.GetService(typeof(AnotherStubService)), Is.Null);
                provider.UnregisterService(typeof(IStubService));
                Assert.That(provider.GetService<IStubService>(), Is.Null);
            }
        }


        private interface IStubService
        {
            string Name { get; }
        }

        private class StubService: IStubService
        {
            public StubService(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private class AnotherStubService: IStubService
        {
            public AnotherStubService() : this("C")
            {

            }

            public AnotherStubService(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}
