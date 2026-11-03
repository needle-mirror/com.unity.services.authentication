#if UNITY_PIPELINE_AVAILABLE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Services.Authentication.Editor;
using Commands = Unity.Services.Authentication.Cli.PlayerAccountsEditorCliCommands;

namespace Unity.Services.Authentication.Cli.Tests
{
    class PlayerAccountsEditorCliCommandsTests
    {
        const string k_ProjectId = "11111111-2222-3333-4444-555555555555";
        const string k_ClientId = "client-1";

        FakeAdminClient m_Client;
        Func<TimeSpan, Task> m_OriginalDelay;
        Func<Func<TimeSpan>> m_OriginalStartClock;
        List<TimeSpan> m_Delays;
        TimeSpan m_Elapsed;

        [SetUp]
        public void SetUp()
        {
            m_Client = new FakeAdminClient();
            m_Delays = new List<TimeSpan>();
            m_Elapsed = TimeSpan.Zero;
            m_OriginalDelay = Commands.Delay;
            m_OriginalStartClock = Commands.StartClock;
            Commands.Delay = delay =>
            {
                m_Delays.Add(delay);
                m_Elapsed += delay;
                m_Client.OnDelay?.Invoke();
                return Task.CompletedTask;
            };
            Commands.StartClock = () => () => m_Elapsed;
        }

        [TearDown]
        public void TearDown()
        {
            Commands.Delay = m_OriginalDelay;
            Commands.StartClock = m_OriginalStartClock;
        }

        static T Run<T>(Task<T> task) => task.GetAwaiter().GetResult();

        static PlayerAccountsClientRequest Request(string name = "My Game", params string[] platforms) => new PlayerAccountsClientRequest
        {
            Name = name,
            Platforms = platforms.Length == 0 ? new[] { "mobile" } : platforms,
        };

        [Test]
        public void Enable_ReturnsTheClientIdAndSettings()
        {
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId };

            var result = Run(Commands.EnableAsync(m_Client, Request("My Game", "mobile", "desktop")));

            Assert.That(result.ProjectId, Is.EqualTo(k_ProjectId));
            Assert.That(result.ClientId, Is.EqualTo(k_ClientId));
            Assert.That(result.Name, Is.EqualTo("My Game"));
            Assert.That(result.Platforms, Is.EqualTo(new[] { "mobile", "desktop" }));
            Assert.That(result.SignInEnabled, Is.True);
            Assert.That(m_Client.LastSaved.Name, Is.EqualTo("My Game"));
            Assert.That(m_Client.LastClientId, Is.EqualTo(k_ClientId), "Reads the client back, since creating it returns no body.");
            Assert.That(m_Delays, Is.Empty);
        }

        [Test]
        public void Enable_WaitsForTheSignInProviderToAppear()
        {
            m_Client.OnDelay = () =>
            {
                if (m_Delays.Count == 2)
                {
                    m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId };
                    m_Client.Clients[k_ClientId] = new PlayerAccountsClientResponse { ClientId = k_ClientId, Name = "My Game", Platforms = new[] { "mobile" } };
                }
            };

            var result = Run(Commands.EnableAsync(m_Client, Request()));

            Assert.That(result.ClientId, Is.EqualTo(k_ClientId));
            Assert.That(m_Delays.Count, Is.EqualTo(2));
        }

        [Test]
        public void Enable_OnARerun_WaitsUntilTheClientShowsTheNewSettings()
        {
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId };
            m_Client.Clients[k_ClientId] = new PlayerAccountsClientResponse { ClientId = k_ClientId, Name = "Old Name", Platforms = new[] { "desktop" } };
            m_Client.ApplySavesLater = true;
            m_Client.OnDelay = () =>
            {
                if (m_Delays.Count == 2)
                    m_Client.ApplyPendingSave();
            };

            var result = Run(Commands.EnableAsync(m_Client, Request("New Name", "desktop", "mobile")));

            Assert.That(result.Name, Is.EqualTo("New Name"));
            Assert.That(result.Platforms, Is.EquivalentTo(new[] { "desktop", "mobile" }));
            Assert.That(m_Delays.Count, Is.EqualTo(2));
        }

        [Test]
        public void Enable_OnAnOlderSetup_WaitsForTheProviderToPointAtTheNewClient()
        {
            // Set up before the clients API: the provider holds the project id, which names no client.
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ProjectId };
            m_Client.SaveTo = k_ClientId;
            m_Client.ApplySavesLater = true;
            m_Client.OnDelay = () =>
            {
                if (m_Delays.Count == 1)
                {
                    m_Client.ApplyPendingSave();
                    m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId };
                }
            };

            var result = Run(Commands.EnableAsync(m_Client, Request()));

            Assert.That(result.ClientId, Is.EqualTo(k_ClientId));
            Assert.That(result.Name, Is.EqualTo("My Game"));
        }

        [Test]
        public void Enable_WhenTheSignInProviderNeverAppears_SaysToRunItAgain()
        {
            var error = Assert.Throws<InvalidOperationException>(() => Run(Commands.EnableAsync(m_Client, Request())));

            Assert.That(error.Message, Does.Contain("player_accounts enable again"));
            Assert.That(error.Message, Does.Contain("sign-in provider did not appear"));
            Assert.That(m_Client.Saves, Is.EqualTo(1));
        }

        [Test]
        public void Enable_WhenTheClientNeverShowsTheSettings_SaysToRunItAgain()
        {
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId };
            m_Client.Clients[k_ClientId] = new PlayerAccountsClientResponse { ClientId = k_ClientId, Name = "Old Name", Platforms = new[] { "mobile" } };
            m_Client.ApplySavesLater = true;

            var error = Assert.Throws<InvalidOperationException>(() => Run(Commands.EnableAsync(m_Client, Request())));

            Assert.That(error.Message, Does.Contain("did not show the new settings"));
        }

        [Test]
        public void Enable_CountsTheTimeSpentInCallsTowardsItsTimeout()
        {
            // Every read takes 10 s, so only a few fit in the 30 s budget however short the delays are.
            m_Client.OnRead = () => m_Elapsed += TimeSpan.FromSeconds(10);

            Assert.Throws<InvalidOperationException>(() => Run(Commands.EnableAsync(m_Client, Request())));

            Assert.That(m_Elapsed, Is.LessThan(TimeSpan.FromSeconds(30 + 10 + 2)));
            Assert.That(m_Delays, Has.All.LessThanOrEqualTo(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void Enable_NeverWaitsPastTheDeadline()
        {
            m_Client.OnRead = () => m_Elapsed += TimeSpan.FromSeconds(1.5);

            Assert.Throws<InvalidOperationException>(() => Run(Commands.EnableAsync(m_Client, Request())));

            Assert.That(m_Elapsed - TimeSpan.FromSeconds(1.5), Is.LessThanOrEqualTo(TimeSpan.FromSeconds(30)), "The last delay ends at the deadline, before the final read.");
        }

        [Test]
        public void Enable_EnablesADisabledSignInProvider()
        {
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId, Disabled = true };

            var result = Run(Commands.EnableAsync(m_Client, Request()));

            Assert.That(m_Client.SignInProviderEnables, Is.EqualTo(1));
            Assert.That(result.SignInEnabled, Is.True);
        }

        [Test]
        public void Reflects_ComparesOnlyTheSettingsTheRequestGives()
        {
            var saved = new PlayerAccountsClientResponse { Name = "My Game", Platforms = new[] { "desktop", "mobile" }, TermsOfServiceLink = "https://example.com/terms" };

            Assert.That(Commands.Reflects(saved, Request("My Game", "mobile", "desktop")), Is.True, "Platforms in any order; links not given are not compared.");
            Assert.That(Commands.Reflects(saved, Request("My Game", "mobile")), Is.False);
            Assert.That(Commands.Reflects(saved, Request("Other Game", "mobile", "desktop")), Is.False);
            Assert.That(Commands.Reflects(saved, new PlayerAccountsClientRequest { PrivacyPolicyLink = "https://example.com/privacy" }), Is.False);
            Assert.That(Commands.Reflects(saved, new PlayerAccountsClientRequest { PrivacyPolicyLink = "" }), Is.True, "An absent link matches its removal.");
        }

        [Test]
        public void Get_FindsTheClientThroughTheSignInProvider()
        {
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId };
            m_Client.Clients[k_ClientId] = new PlayerAccountsClientResponse { ClientId = k_ClientId, Name = "My Game", Platforms = new[] { "desktop" }, TermsOfServiceLink = "https://example.com/terms", PrivacyPolicyLink = "" };

            var result = Run(Commands.GetAsync(m_Client));

            Assert.That(m_Client.LastClientId, Is.EqualTo(k_ClientId));
            Assert.That(result.Status, Is.EqualTo(Commands.PlayerAccountsStatus.Ready));
            Assert.That(result.Name, Is.EqualTo("My Game"));
            Assert.That(result.TermsUrl, Is.EqualTo("https://example.com/terms"));
            Assert.That(result.PrivacyUrl, Is.Null);
        }

        [Test]
        public void Get_WhenPlayerAccountsIsOff_ReportsNotSetUp()
        {
            var result = Run(Commands.GetAsync(m_Client));

            Assert.That(result.Status, Is.EqualTo(Commands.PlayerAccountsStatus.NotSetUp));
            Assert.That(result.ProjectId, Is.EqualTo(k_ProjectId));
            Assert.That(result.ClientId, Is.Null);
            Assert.That(result.SignInEnabled, Is.False);
        }

        [Test]
        public void Get_WhenTheProvidersClientIsMissing_ReportsClientMissingWithoutTheProvidersId()
        {
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ProjectId };

            var result = Run(Commands.GetAsync(m_Client));

            Assert.That(result.Status, Is.EqualTo(Commands.PlayerAccountsStatus.ClientMissing));
            Assert.That(result.ClientId, Is.Null, "An older setup holds the project id here, which is not a Client ID to paste anywhere.");
            Assert.That(result.SignInEnabled, Is.True);
        }

        [Test]
        public void Get_WhenACallFails_ThrowsRatherThanReportingAState()
        {
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId };
            m_Client.Failure = new PlayerAccountsAdminException(PlayerAccountsAdminException.FailureReason.Forbidden, 403, null);

            Assert.Throws<InvalidOperationException>(() => Run(Commands.GetAsync(m_Client)));
        }

        [Test]
        public void Status_IsWrittenAsItsName()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(new Commands.PlayerAccountsDto { Status = Commands.PlayerAccountsStatus.ClientMissing });

            Assert.That(json, Does.Contain("\"Status\":\"ClientMissing\""));
        }

        [Test]
        public void Update_SendsOnlyThePassedSettingsToTheClientFoundThroughTheSignInProvider()
        {
            m_Client.SignInProvider = new IdProviderResponse { Type = "unity", ClientId = k_ClientId };
            var changes = new PlayerAccountsClientRequest { Platforms = new[] { "desktop" } };

            Run(Commands.UpdateAsync(m_Client, changes));

            Assert.That(m_Client.LastSaved, Is.SameAs(changes));
            Assert.That(m_Client.LastClientId, Is.EqualTo(k_ClientId));
        }

        [Test]
        public void UpdateCommand_WithNothingToChange_IsRefusedBeforeAnyCall()
        {
            var error = Assert.Throws<ArgumentException>(() => Commands.Update());

            Assert.That(error.Message, Does.Contain("--name"));
        }

        [TestCase("mobile,desktop", new[] { "mobile", "desktop" })]
        [TestCase(" Desktop , mobile, desktop ", new[] { "desktop", "mobile" })]
        public void ParsePlatforms_NormalizesAndDeduplicates(string input, string[] expected)
        {
            Assert.That(Commands.ParsePlatforms(input), Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase(" , ")]
        [TestCase("mobile,console")]
        public void ParsePlatforms_RejectsMissingOrUnknownPlatforms(string input)
        {
            var error = Assert.Throws<ArgumentException>(() => Commands.ParsePlatforms(input));

            Assert.That(error.Message, Does.Contain("mobile, desktop"));
        }

        [TestCase("ftp://example.com/terms")]
        [TestCase("example.com/terms")]
        public void OptionalUrl_RejectsAnythingButAWebAddress(string url)
        {
            var error = Assert.Throws<ArgumentException>(() => Commands.OptionalUrl("terms-url", url));

            Assert.That(error.Message, Does.Contain("--terms-url"));
        }

        [Test]
        public void OptionalUrl_TrimsAndKeepsTheLinkWhenNotGiven()
        {
            Assert.That(Commands.OptionalUrl("terms-url", " https://example.com/terms "), Is.EqualTo("https://example.com/terms"));
            Assert.That(Commands.OptionalUrl("terms-url", null), Is.Null, "Not given: keep the link.");
        }

        [TestCase("")]
        [TestCase(" ")]
        public void OptionalUrl_RefusesAnEmptyLinkAndPointsAtTheRemoveFlag(string url)
        {
            var error = Assert.Throws<ArgumentException>(() => Commands.OptionalUrl("terms-url", url));

            Assert.That(error.Message, Does.Contain("--remove-terms-url true"));
        }

        [Test]
        public void LinkChange_RemovesOnlyWhenAskedTo()
        {
            Assert.That(Commands.LinkChange("terms-url", null, remove: true), Is.Empty);
            Assert.That(Commands.LinkChange("terms-url", null, remove: false), Is.Null);
            Assert.That(Commands.LinkChange("terms-url", "https://example.com/terms", remove: false), Is.EqualTo("https://example.com/terms"));
            Assert.Throws<ArgumentException>(() => Commands.LinkChange("terms-url", "https://example.com/terms", remove: true));
        }

        [TestCase("ab")]
        [TestCase(" ")]
        public void RequiredName_RejectsNamesTheServiceWouldRefuse(string name)
        {
            Assert.Throws<ArgumentException>(() => Commands.RequiredName(name));
        }

        [Test]
        public void RequiredName_RejectsNamesOver128Characters()
        {
            Assert.Throws<ArgumentException>(() => Commands.RequiredName(new string('a', 129)));
            Assert.That(Commands.RequiredName(" abc "), Is.EqualTo("abc"));
        }

        [TestCase("Apple", "apple")]
        [TestCase("google", "google")]
        [TestCase("facebook", "facebook")]
        public void ParseProvider_AcceptsTheSupportedProviders(string input, string expected)
        {
            Assert.That(Commands.ParseProvider(input), Is.EqualTo(expected));
        }

        [Test]
        public void ParseProvider_RejectsOthers()
        {
            var error = Assert.Throws<ArgumentException>(() => Commands.ParseProvider("steam"));

            Assert.That(error.Message, Does.Contain("apple, google, facebook"));
        }

        [Test]
        public void ListIdentityProviders_ReportsEachWithItsState()
        {
            m_Client.IdentityProviders = new[]
            {
                new PlayerAccountsIdentityProviderResponse { Id = "idp-1", Provider = "apple", ProviderClientId = "com.example" },
                new PlayerAccountsIdentityProviderResponse { Id = "idp-2", Provider = "google", ProviderClientId = "g", Disabled = true },
            };

            var result = Run(Commands.ListIdentityProvidersAsync(m_Client));

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.IdentityProviders.ConvertAll(p => (p.Id, p.ProviderClientId, p.Enabled)),
                Is.EqualTo(new[] { ("idp-1", "com.example", true), ("idp-2", "g", false) }));
        }

        [Test]
        public void CreateIdentityProvider_WithoutAnId_SaysToCheckBeforeRetrying()
        {
            m_Client.CreatedIdentityProvider = new PlayerAccountsIdentityProviderResponse();

            var error = Assert.Throws<InvalidOperationException>(() => Run(Commands.CreateIdentityProviderAsync(m_Client,
                new CreatePlayerAccountsIdentityProviderRequest { Provider = "apple", ProviderClientId = "com.example" })));

            Assert.That(error.Message, Does.Contain("list_identity_providers"));
        }

        [Test]
        public void EnableAndDisableIdentityProvider_UpdateItsDisabledFlag()
        {
            var enabled = Run(Commands.EnableIdentityProviderAsync(m_Client, " idp-1 "));
            var disabled = Run(Commands.DisableIdentityProviderAsync(m_Client, "idp-1", confirm: true));

            Assert.That(m_Client.Toggles, Is.EqualTo(new[] { ("idp-1", true), ("idp-1", false) }));
            Assert.That(enabled.Enabled, Is.True);
            Assert.That(disabled.Enabled, Is.False);
        }

        [Test]
        public void DisableIdentityProvider_WithoutConfirm_IsRefusedAndChangesNothing()
        {
            var error = Assert.Throws<ArgumentException>(() => Run(Commands.DisableIdentityProviderAsync(m_Client, "idp-1", confirm: false)));

            Assert.That(error.Message, Does.Contain("--confirm true"));
            Assert.That(error.Message, Does.Contain("stops"));
            Assert.That(m_Client.Calls, Is.Zero);
        }

        [Test]
        public void DeleteIdentityProvider_WithoutConfirm_IsRefusedAndDeletesNothing()
        {
            var error = Assert.Throws<ArgumentException>(() => Run(Commands.DeleteIdentityProviderAsync(m_Client, "idp-1", confirm: false, dryRun: false)));

            Assert.That(error.Message, Does.Contain("--confirm true"));
            Assert.That(error.Message, Does.Contain("--dry-run true"));
            Assert.That(error.Message, Does.Contain("disable_identity_provider"));
            Assert.That(m_Client.Deleted, Is.Empty);
        }

        [Test]
        public void DeleteIdentityProvider_WithConfirm_DeletesAndReportsTheApp()
        {
            m_Client.IdentityProviders = new[]
            {
                new PlayerAccountsIdentityProviderResponse { Id = "idp-1", Provider = "apple", ProviderClientId = "com.example" },
            };

            var result = Run(Commands.DeleteIdentityProviderAsync(m_Client, "idp-1", confirm: true, dryRun: false));

            Assert.That(result.Deleted, Is.True);
            Assert.That(result.DryRun, Is.False);
            Assert.That(result.Provider, Is.EqualTo("apple"));
            Assert.That(result.ProviderClientId, Is.EqualTo("com.example"));
            Assert.That(m_Client.Deleted, Is.EqualTo(new[] { "idp-1" }));
        }

        [Test]
        public void DeleteIdentityProvider_ForAnUnknownId_NamesItAndDeletesNothing()
        {
            var error = Assert.Throws<ArgumentException>(() => Run(Commands.DeleteIdentityProviderAsync(m_Client, "missing", confirm: true, dryRun: false)));

            Assert.That(error.Message, Does.Contain("'missing'"));
            Assert.That(m_Client.Deleted, Is.Empty);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DeleteIdentityProvider_DryRun_ReportsTheAppAndDeletesNothingEvenWithConfirm(bool confirm)
        {
            m_Client.IdentityProviders = new[]
            {
                new PlayerAccountsIdentityProviderResponse { Id = "idp-1", Provider = "apple", ProviderClientId = "com.example" },
            };

            var result = Run(Commands.DeleteIdentityProviderAsync(m_Client, "idp-1", confirm, dryRun: true));

            Assert.That(result.DryRun, Is.True);
            Assert.That(result.Deleted, Is.False);
            Assert.That(result.Provider, Is.EqualTo("apple"));
            Assert.That(result.ProviderClientId, Is.EqualTo("com.example"));
            Assert.That(m_Client.Deleted, Is.Empty);
        }

        [Test]
        public void DeleteIdentityProvider_DryRunForAnUnknownId_NamesTheIdAndTheListCommand()
        {
            var error = Assert.Throws<ArgumentException>(() => Run(Commands.DeleteIdentityProviderAsync(m_Client, "missing", confirm: false, dryRun: true)));

            Assert.That(error.Message, Does.Contain("'missing'"));
            Assert.That(error.Message, Does.Contain("list_identity_providers"));
        }

        [Test]
        public void MissingId_IsRefusedBeforeAnyCall()
        {
            Assert.Throws<ArgumentException>(() => Run(Commands.EnableIdentityProviderAsync(m_Client, " ")));
            Assert.Throws<ArgumentException>(() => Run(Commands.DisableIdentityProviderAsync(m_Client, " ", confirm: true)));
            Assert.Throws<ArgumentException>(() => Run(Commands.DeleteIdentityProviderAsync(m_Client, null, confirm: true, dryRun: false)));
            Assert.That(m_Client.Calls, Is.Zero);
        }

        [Test]
        public void Forbidden_NamesTheProjectAndTheRoleToAskFor()
        {
            m_Client.Failure = new PlayerAccountsAdminException(PlayerAccountsAdminException.FailureReason.Forbidden, 403, "Missing permission");

            var error = Assert.Throws<InvalidOperationException>(() => Run(Commands.ListIdentityProvidersAsync(m_Client)));

            Assert.That(error.Message, Does.Contain(k_ProjectId));
            Assert.That(error.Message, Does.Contain("Authentication Editor"));
            Assert.That(error.Message, Does.Contain("Administration > Members"));
            Assert.That(error.Message, Does.Contain("Missing permission"));
        }

        [Test]
        public void NotFoundForAnIdentityProvider_NamesTheIdAndTheListCommand()
        {
            m_Client.Failure = new PlayerAccountsAdminException(PlayerAccountsAdminException.FailureReason.NotFound, 404, null);

            var error = Assert.Throws<ArgumentException>(() => Run(Commands.EnableIdentityProviderAsync(m_Client, "missing")));

            Assert.That(error.Message, Does.Contain("'missing'"));
            Assert.That(error.Message, Does.Contain("list_identity_providers"));
        }

        [Test]
        public void Unauthorized_SaysToSignInAgain()
        {
            m_Client.Failure = new PlayerAccountsAdminException(PlayerAccountsAdminException.FailureReason.Unauthorized, 401, null);

            var error = Assert.Throws<InvalidOperationException>(() => Run(Commands.GetAsync(m_Client)));

            Assert.That(error.Message, Does.Contain("Sign in again"));
        }

        class FakeAdminClient : IPlayerAccountsAdminClient
        {
            public IdProviderResponse SignInProvider;
            public Dictionary<string, PlayerAccountsClientResponse> Clients = new Dictionary<string, PlayerAccountsClientResponse>();
            public PlayerAccountsIdentityProviderResponse[] IdentityProviders = Array.Empty<PlayerAccountsIdentityProviderResponse>();
            public PlayerAccountsIdentityProviderResponse CreatedIdentityProvider = new PlayerAccountsIdentityProviderResponse { Id = "idp-new" };
            public PlayerAccountsAdminException Failure;
            public Action OnDelay;
            /// <summary>Runs on every read of the sign-in provider or the client, standing in for its latency.</summary>
            public Action OnRead;

            /// <summary>Where an accepted create-or-update lands; the sign-in provider's client by default.</summary>
            public string SaveTo;
            /// <summary>Holds an accepted create-or-update back until <see cref="ApplyPendingSave"/>, as the service may.</summary>
            public bool ApplySavesLater;
            PlayerAccountsClientRequest m_Pending;

            public PlayerAccountsClientRequest LastSaved;
            public string LastClientId;
            public int Saves;
            public int SignInProviderEnables;
            public List<(string, bool)> Toggles = new List<(string, bool)>();
            public List<string> Deleted = new List<string>();
            public int Calls;

            public string ProjectId => k_ProjectId;

            Task<T> Respond<T>(Func<T> result)
            {
                Calls++;
                return Failure != null ? Task.FromException<T>(Failure) : Task.FromResult(result());
            }

            public void ApplyPendingSave()
            {
                Save(SaveTo ?? SignInProvider?.ClientId, m_Pending);
                m_Pending = null;
            }

            PlayerAccountsClientResponse Save(string clientId, PlayerAccountsClientRequest request)
            {
                Clients.TryGetValue(clientId ?? "", out var stored);
                var saved = new PlayerAccountsClientResponse
                {
                    ClientId = clientId,
                    Name = request.Name ?? stored?.Name,
                    Platforms = request.Platforms ?? stored?.Platforms,
                    TermsOfServiceLink = request.TermsOfServiceLink ?? stored?.TermsOfServiceLink,
                    PrivacyPolicyLink = request.PrivacyPolicyLink ?? stored?.PrivacyPolicyLink,
                };
                if (clientId != null)
                {
                    Clients[clientId] = saved;
                }

                return saved;
            }

            public Task CreateOrUpdateClientAsync(PlayerAccountsClientRequest request) => Respond(() =>
            {
                Saves++;
                LastSaved = request;
                if (ApplySavesLater)
                {
                    m_Pending = request;
                }
                else
                {
                    Save(SaveTo ?? SignInProvider?.ClientId, request);
                }

                return true;
            });

            public Task<PlayerAccountsClientResponse> GetClientAsync(string clientId)
            {
                OnRead?.Invoke();
                LastClientId = clientId;
                if (Failure == null && !Clients.ContainsKey(clientId))
                {
                    Calls++;
                    return Task.FromException<PlayerAccountsClientResponse>(
                        new PlayerAccountsAdminException(PlayerAccountsAdminException.FailureReason.NotFound, 404, null));
                }

                return Respond(() => Clients[clientId]);
            }

            public Task<PlayerAccountsClientResponse> UpdateClientAsync(string clientId, PlayerAccountsClientRequest request) => Respond(() =>
            {
                LastClientId = clientId;
                LastSaved = request;
                return Save(clientId, request);
            });

            public Task<IdProviderResponse> GetUnitySignInProviderAsync()
            {
                OnRead?.Invoke();
                return Respond(() => SignInProvider);
            }

            public Task<IdProviderResponse> EnableUnitySignInProviderAsync() => Respond(() =>
            {
                SignInProviderEnables++;
                SignInProvider.Disabled = false;
                return SignInProvider;
            });

            public Task<PlayerAccountsIdentityProviderResponse[]> ListIdentityProvidersAsync() => Respond(() => IdentityProviders);

            public Task<PlayerAccountsIdentityProviderResponse> CreateIdentityProviderAsync(CreatePlayerAccountsIdentityProviderRequest request) => Respond(() => CreatedIdentityProvider);

            public Task<PlayerAccountsIdentityProviderResponse> UpdateIdentityProviderAsync(string id, UpdatePlayerAccountsIdentityProviderRequest request) =>
                Respond(() => Toggle(id, request.Disabled == false));

            PlayerAccountsIdentityProviderResponse Toggle(string id, bool enabled)
            {
                Toggles.Add((id, enabled));
                return new PlayerAccountsIdentityProviderResponse { Id = id, Disabled = !enabled };
            }

            public Task DeleteIdentityProviderAsync(string id) => Respond(() =>
            {
                Deleted.Add(id);
                return true;
            });
        }
    }
}
#endif
