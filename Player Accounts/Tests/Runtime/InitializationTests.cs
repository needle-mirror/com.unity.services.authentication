#if NUGET_MOQ_AVAILABLE && UNITY_EDITOR
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Moq;
using NUnit.Framework;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Services.Core.TestUtils;
using UnityEngine.TestTools;

namespace Unity.Services.Authentication.PlayerAccounts.Tests
{
    class InitializationTests
    {
        const string k_AssetPath = "Assets/Resources";
        PlayerAccountsPackageInitializer m_Package;
        // Start is called before the first frame update

        [SetUp]
        public void Setup()
        {
            LoadOrCreateSettings();
            m_Package = new PlayerAccountsPackageInitializer();
            PlayerAccountService.Instance = null;
        }

        [TearDown]
        public void TearDown()
        {
            PlayerAccountService.Instance = null;
        }

        static void LoadOrCreateSettings()
        {
            var settings = UnityPlayerAccountSettings.Load();

            if (settings == null)
            {
                if (!AssetDatabase.IsValidFolder(k_AssetPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                settings = ScriptableObject.CreateInstance<UnityPlayerAccountSettings>();
                AssetDatabase.CreateAsset(settings, $"{k_AssetPath}/{nameof(UnityPlayerAccountSettings)}.asset");
            }
        }

        /// <summary>
        /// A test to validate that your package initializer behaves as expected when ran in optimal conditions.
        /// </summary>
        [UnityTest]
        public IEnumerator InitializeInOptimistPathSucceeds()
        {
            // ServicesCoreScope is an utility to simulate a Core context (UnityServices, CoreRegistry, ...)
            // Checkout our handbook page for more info: https://pages.prd.mz.internal.unity3d.com/mz-developer-handbook/docs/sdk/testing/core-services-test-utilities
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
                Assert.IsNotNull(PlayerAccountService.Instance);
            }
        }

        /// <summary>
        /// A test to validate that your package initializer behaves as expected when initialized multiple times.
        /// </summary>
        [UnityTest]
        public IEnumerator InitializeTwicePutsServiceInExpectedState()
        {
            // ServicesCoreScope is an utility to simulate a Core context (UnityServices, CoreRegistry, ...)
            // Checkout our handbook page for more info: https://pages.prd.mz.internal.unity3d.com/mz-developer-handbook/docs/sdk/testing/core-services-test-utilities
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
                Assert.IsNotNull(PlayerAccountService.Instance);

                // Setup before 2nd initialization: This is specific for each service. In this example we make
                // sure each initialization sets a different instance to OperateTemplateService.Instance.
                var firstInitializationInstance = PlayerAccountService.Instance;

                // Re-initialize your package using the simulated Core.
                initialization = testCore.InitializePackageAsync(m_Package);
                while (!initialization.IsCompleted)
                {
                    yield return null;
                }

                // Assert your initializer behaved as expected on a second call.
                Assert.AreEqual(TaskStatus.RanToCompletion, initialization.Status);
                Assert.IsNotNull(PlayerAccountService.Instance);
                Assert.AreNotSame(firstInitializationInstance, PlayerAccountService.Instance);
            }
        }

        /// <summary>
        /// Register fake providers for all dependencies of <see cref="TemplatePackageInitializer"/>.
        /// </summary>
        static void RegisterFakeProviders(ServicesCoreScope testCore)
        {
            testCore.RegisterProviderFor<ICloudProjectId>(CreateFakeCloudProjectId());

            ICloudProjectId CreateFakeCloudProjectId()
            {
                var mock = new Mock<ICloudProjectId>();
                mock.Setup(x => x.GetCloudProjectId())
                    .Returns("");
                return mock.Object;
            }
        }
    }
}
#endif
