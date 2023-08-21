#if NUGET_MOQ_AVAILABLE && UNITY_EDITOR
using System.Collections;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Services.Core.TestUtils;
using UnityEngine.TestTools;

namespace Unity.Services.Authentication.Server.Tests
{
    class ServerInitializationTests
    {
        ServerAuthenticationInitializer m_Package;

        [SetUp]
        public void Setup()
        {
            m_Package = new ServerAuthenticationInitializer();
            ServerAuthenticationService.Instance = null;
        }

        [TearDown]
        public void TearDown()
        {
            ServerAuthenticationService.Instance = null;
        }

        /// <summary>
        /// A test to validate that your package initializer behaves as expected when ran in optimal conditions.
        /// </summary>
        [UnityTest]
        public IEnumerator InitializeSucceeds()
        {
            using (var testCore = new ServicesCoreScope())
            {
                RegisterFakeProviders(testCore);

                var initialization = testCore.InitializePackageAsync(m_Package);

                while (!initialization.IsCompleted)
                {
                    yield return null;
                }

                Assert.AreEqual(TaskStatus.RanToCompletion, initialization.Status);
                Assert.IsNotNull(ServerAuthenticationService.Instance);
            }
        }

        /// <summary>
        /// A test to validate that your package initializer behaves as expected when initialized multiple times.
        /// </summary>
        [UnityTest]
        public IEnumerator InitializeTwicePutsServiceInExpectedState()
        {
            using (var testCore = new ServicesCoreScope())
            {
                // Be sure to register providers for your package's dependencies if you want to test the happy path.
                RegisterFakeProviders(testCore);

                // Initialize your package using the simulated Core.
                var initialization = testCore.InitializePackageAsync(m_Package);

                // Since task tests are not supported yet, we have to manually yield until the task completes.
                while (!initialization.IsCompleted)
                {
                    yield return null;
                }

                // Assert your initializer behaved as expected.
                Assert.AreEqual(TaskStatus.RanToCompletion, initialization.Status);
                Assert.IsNotNull(ServerAuthenticationService.Instance);

                // Setup before 2nd initialization: This is specific for each service. In this example we make
                // sure each initialization sets a different instance to OperateTemplateService.Instance.
                var firstInitializationInstance = ServerAuthenticationService.Instance;

                // Re-initialize your package using the simulated Core.
                initialization = testCore.InitializePackageAsync(m_Package);
                while (!initialization.IsCompleted)
                {
                    yield return null;
                }

                // Assert your initializer behaved as expected on a second call.
                Assert.AreEqual(TaskStatus.RanToCompletion, initialization.Status);
                Assert.IsNotNull(ServerAuthenticationService.Instance);
                Assert.AreNotSame(firstInitializationInstance, ServerAuthenticationService.Instance);
            }
        }

        static void RegisterFakeProviders(ServicesCoreScope testCore)
        {
            testCore.RegisterProviderFor<IEnvironments>(CreateFakeEnvironments());
            testCore.RegisterProviderFor<IActionScheduler>(CreateFakeActionScheduler());
            testCore.RegisterProviderFor<ICloudProjectId>(CreateFakeCloudProjectId());
            testCore.RegisterProviderFor<IProjectConfiguration>(CreateFakeProjectConfiguration());
        }

        static IActionScheduler CreateFakeActionScheduler()
        {
            var mock = new Mock<IActionScheduler>();
            return mock.Object;
        }

        static IProjectConfiguration CreateFakeProjectConfiguration()
        {
            var mock = new Mock<IProjectConfiguration>();
            mock.Setup(x => x.GetBool(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((key, defaultValue) => defaultValue);
            return mock.Object;
        }

        static ICloudProjectId CreateFakeCloudProjectId()
        {
            var mock = new Mock<ICloudProjectId>();
            mock.Setup(x => x.GetCloudProjectId())
                .Returns("");
            return mock.Object;
        }

        static IEnvironments CreateFakeEnvironments()
        {
            var mock = new Mock<IEnvironments>();
            mock.Setup(x => x.Current)
                .Returns("");
            return mock.Object;
        }
    }
}
#endif
