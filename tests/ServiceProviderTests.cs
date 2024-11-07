using Minimal.Mvvm;
using System.Xml.Linq;

namespace NuExt.Minimal.Mvvm.Tests
{
    internal class ServiceProviderTests
    {

        [Test]
        public void ServiceContainerTest()
        {
            IServiceContainer provider = new ServiceProvider();

            Assert.That(typeof(StubServiceBase).IsAssignableFrom(typeof(StubService)), Is.True);
            Assert.That(typeof(StubService).IsAssignableFrom(typeof(StubServiceBase)), Is.False);
            Assert.That(typeof(StubService).IsAssignableFrom(typeof(IStubService)), Is.False);
            Assert.That(typeof(IStubService).IsAssignableFrom(typeof(StubService)), Is.True);

            provider.RegisterService(new StubService("A"));
            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<StubServiceBase>, Is.Not.Null);
                Assert.That(provider.GetService<StubServiceBase>()!.Name, Is.EqualTo("A"));
                Assert.That(provider.GetService<IStubService>, Is.Not.Null);
                Assert.That(provider.GetService<IStubService>()!.Name, Is.EqualTo("A"));
            });
            provider.RegisterService<IStubService>(new StubService("B"));

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<StubService>, Is.Not.Null);
                Assert.That(provider.GetService<StubService>()!.Name, Is.EqualTo("A"));
                Assert.That(provider.GetService<IStubService>, Is.Not.Null);
                Assert.That(provider.GetService<IStubService>()!.Name, Is.EqualTo("B"));
                Assert.That(provider.GetServices(typeof(StubServiceBase)), Is.Not.Null);
                Assert.That(provider.GetServices(typeof(StubServiceBase)).Count(), Is.EqualTo(2));
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
                    Assert.That(provider.GetServices(typeof(StubServiceBase)), Is.Not.Null);
                    Assert.That(provider.GetServices(typeof(StubServiceBase)).Count(), Is.EqualTo(3));
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
                bool res = provider.UnregisterService<AnotherStubService>();
                Assert.Multiple(() =>
                {
                    Assert.That(res, Is.True);
                    Assert.That(provider.GetService(typeof(AnotherStubService)), Is.Not.Null);
                    Assert.That(((AnotherStubService?)provider.GetService(typeof(AnotherStubService)))!.Name, Is.EqualTo("D"));
                });
                res = provider.UnregisterService(typeof(IStubService));
                Assert.Multiple(() =>
                {
                    Assert.That(res, Is.True);
                    Assert.That(provider.GetService<IStubService>, Is.Not.Null);
                    Assert.That(provider.GetService<IStubService>()!.Name, Is.EqualTo("A"));
                });
            }
        }


        private interface IStubService
        {
            string Name { get; }
        }

        private abstract class StubServiceBase
        {
            protected StubServiceBase(string name)
            {
                Name = name;
            }
            public string Name { get; }
        }

        private class StubService: StubServiceBase, IStubService
        {
            public StubService(string name) : base(name)
            {
            }
        }

        private class AnotherStubService: StubServiceBase, IStubService
        {
            public AnotherStubService() : this("C")
            {

            }

            public AnotherStubService(string name):base(name)
            {
            }
        }
    }
}
