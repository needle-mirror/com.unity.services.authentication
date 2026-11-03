#if UNITY_PIPELINE_AVAILABLE && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Pipeline.Commands;
using Unity.Services.Authentication.Editor;

namespace Unity.Services.Authentication.Cli
{
    /// <summary>
    /// Pipeline CLI commands to set up Unity Player Accounts for the Editor's linked Unity Cloud project without the
    /// Dashboard: turn it on, read and change its settings, and manage the game's own social provider apps.
    /// </summary>
    /// <remarks>
    /// They mirror what the Dashboard already offers. A project has one Player Accounts client, so no command takes a
    /// client id: it is found through the project's Unity Player Accounts sign-in provider in Authentication.
    /// </remarks>
    static class PlayerAccountsEditorCliCommands
    {
        const string k_Group = AuthenticationCliConstants.PlayerAccountsGroup;
        const string k_EnableCommand = k_Group + " enable";
        const string k_GetCommand = k_Group + " get";
        const string k_UpdateCommand = k_Group + " update";
        const string k_ListIdentityProvidersCommand = k_Group + " list_identity_providers";
        const string k_DisableIdentityProviderCommand = k_Group + " disable_identity_provider";

        static readonly TimeSpan k_SetupTimeout = TimeSpan.FromSeconds(AuthenticationCliConstants.PlayerAccountsSetupTimeoutSeconds);
        static readonly TimeSpan k_SetupPollInterval = TimeSpan.FromSeconds(AuthenticationCliConstants.PlayerAccountsSetupPollIntervalSeconds);

        internal static Func<TimeSpan, Task> Delay = Task.Delay;

        /// <summary>Starts a monotonic clock and returns a function that reads the time elapsed since.</summary>
        internal static Func<Func<TimeSpan>> StartClock = () =>
        {
            var stopwatch = Stopwatch.StartNew();
            return () => stopwatch.Elapsed;
        };

        /// <summary>What a call's id names, so a missing one is reported as the right thing.</summary>
        internal enum Subject
        {
            Project,
            Client,
            IdentityProvider,
        }

        [CliGroupCommand(k_Group,
            "Turn Unity Player Accounts on for the project this Editor is linked to, and return its Client ID, the value to set in Services > Unity Player Accounts. Safe to run again: it creates the project's client, or updates the existing one, and waits until the client shows the settings given")]
        public static Task<PlayerAccountsDto> Enable(
            [CliArg("name", "Name players see when they sign in, usually the game's name", Required = true)] string name,
            [CliArg("platforms", "Comma-separated platforms the game signs players in on: " + AuthenticationCliConstants.PlayerAccountsPlatformList, Required = true)] string platforms,
            [CliArg("terms-url", "Optional link to the game's terms of service, shown at sign-in")] string termsUrl = null,
            [CliArg("privacy-url", "Optional link to the game's privacy policy, shown at sign-in")] string privacyUrl = null)
        {
            var request = new PlayerAccountsClientRequest
            {
                Name = RequiredName(name),
                Platforms = ParsePlatforms(platforms),
                TermsOfServiceLink = OptionalUrl("terms-url", termsUrl),
                PrivacyPolicyLink = OptionalUrl("privacy-url", privacyUrl),
            };
            return EnableAsync(CreateClient(), request);
        }

        [CliGroupCommand(k_Group,
            "Show the project's Unity Player Accounts setup: Status (Ready, NotSetUp, or ClientMissing when the sign-in provider points at a client Player Accounts does not have; run enable for either), and when Ready the Client ID, name, platforms, terms and privacy links, and whether Player Accounts sign-in is enabled")]
        public static Task<PlayerAccountsDto> Get()
        {
            return GetAsync(CreateClient());
        }

        [CliGroupCommand(k_Group,
            "Change the project's Unity Player Accounts settings. Only the options given change; the others keep their values. To remove a link, pass --remove-terms-url true or --remove-privacy-url true")]
        public static Task<PlayerAccountsDto> Update(
            [CliArg("name", "Name players see when they sign in")] string name = null,
            [CliArg("platforms", "Comma-separated platforms, replacing the current ones: " + AuthenticationCliConstants.PlayerAccountsPlatformList)] string platforms = null,
            [CliArg("terms-url", "New link to the game's terms of service")] string termsUrl = null,
            [CliArg("privacy-url", "New link to the game's privacy policy")] string privacyUrl = null,
            [CliArg("remove-terms-url", "True to remove the terms of service link")] bool removeTermsUrl = false,
            [CliArg("remove-privacy-url", "True to remove the privacy policy link")] bool removePrivacyUrl = false)
        {
            var changes = new PlayerAccountsClientRequest
            {
                Name = name == null ? null : RequiredName(name),
                Platforms = platforms == null ? null : ParsePlatforms(platforms),
                TermsOfServiceLink = LinkChange("terms-url", termsUrl, removeTermsUrl),
                PrivacyPolicyLink = LinkChange("privacy-url", privacyUrl, removePrivacyUrl),
            };
            if (changes.Name == null && changes.Platforms == null && changes.TermsOfServiceLink == null && changes.PrivacyPolicyLink == null)
            {
                throw new ArgumentException("Nothing to change: pass at least one of --name, --platforms, --terms-url, --privacy-url, --remove-terms-url or --remove-privacy-url.");
            }

            return UpdateAsync(CreateClient(), changes);
        }

        [CliGroupCommand(k_Group,
            "List the game's own social provider apps registered with Unity Player Accounts (Apple, Google, Facebook), with their ids and whether each is enabled. Unity's shared apps are accepted whether or not any are registered")]
        public static Task<IdentityProviderListDto> ListIdentityProviders()
        {
            return ListIdentityProvidersAsync(CreateClient());
        }

        [CliGroupCommand(k_Group,
            "Register the game's own social provider app with Unity Player Accounts, so Player Accounts also accepts sign-in through that app. Unity's shared app keeps working alongside it; the game chooses which one it signs in with")]
        public static Task<IdentityProviderDto> CreateIdentityProvider(
            [CliArg("provider", "The social provider: " + AuthenticationCliConstants.PlayerAccountsIdentityProviderList, Required = true)] string provider,
            [CliArg("provider-client-id", "The app's client id from the provider's developer console, not the Player Accounts Client ID", Required = true)] string providerClientId)
        {
            var request = new CreatePlayerAccountsIdentityProviderRequest
            {
                Provider = ParseProvider(provider),
                ProviderClientId = Required("provider-client-id", providerClientId, "the app's client id from the provider's developer console", AuthenticationCliConstants.PlayerAccountsMaxProviderClientIdLength),
            };
            return CreateIdentityProviderAsync(CreateClient(), request);
        }

        [CliGroupCommand(k_Group,
            "Enable sign-in through one of the game's registered social provider apps")]
        public static Task<IdentityProviderDto> EnableIdentityProvider(
            [CliArg("id", "The registered app's id, as " + k_ListIdentityProvidersCommand + " reports it", Required = true)] string id)
        {
            return EnableIdentityProviderAsync(CreateClient(), id);
        }

        [CliGroupCommand(k_Group,
            "Disable sign-in through one of the game's registered social provider apps. A game signing in through that app stops being able to at once: it does not fall back to Unity's shared app. Requires --confirm true")]
        public static Task<IdentityProviderDto> DisableIdentityProvider(
            [CliArg("id", "The registered app's id, as " + k_ListIdentityProvidersCommand + " reports it", Required = true)] string id,
            [CliArg("confirm", "Must be true to disable: sign-in through that app stops for players at once")] bool confirm = false)
        {
            return DisableIdentityProviderAsync(CreateClient(), id, confirm);
        }

        [CliGroupCommand(k_Group,
            "Delete one of the game's registered social provider apps. Destructive: requires --confirm true (use --dry-run true to check which app the id names first). To stop using it for now, disable it instead")]
        public static Task<IdentityProviderDeletedDto> DeleteIdentityProvider(
            [CliArg("id", "The registered app's id, as " + k_ListIdentityProvidersCommand + " reports it", Required = true)] string id,
            [CliArg("confirm", "Must be true to delete (destructive guard)")] bool confirm = false,
            [CliArg("dry-run", "Report the provider and provider client id the id names without deleting anything. Wins over --confirm")] bool dryRun = false)
        {
            return DeleteIdentityProviderAsync(CreateClient(), id, confirm, dryRun);
        }

        internal static async Task<PlayerAccountsDto> EnableAsync(IPlayerAccountsAdminClient client, PlayerAccountsClientRequest request)
        {
            await Call(client, () => client.CreateOrUpdateClientAsync(request), Subject.Project, id: null);

            // The service only accepts the request. Once the client is saved, the backend points the project's Unity
            // Player Accounts sign-in provider in Authentication at it. A provider that exists is not proof of that: a
            // project set up before the clients API has one holding the project id, and a re-run finds the previous
            // settings. So poll until the provider's client shows what was asked for.
            var elapsed = StartClock();
            while (true)
            {
                var signInProvider = await Call(client, () => client.GetUnitySignInProviderAsync(), Subject.Project, id: null);
                var clientId = signInProvider == null ? null : signInProvider.ClientId;
                if (!string.IsNullOrEmpty(clientId))
                {
                    var saved = await FindClientAsync(client, clientId);
                    if (saved != null && Reflects(saved, request))
                    {
                        if (signInProvider.Disabled)
                        {
                            signInProvider = await Call(client, () => client.EnableUnitySignInProviderAsync(), Subject.Project, id: null) ?? signInProvider;
                        }

                        return PlayerAccountsDto.From(client.ProjectId, clientId, saved, signInProvider);
                    }
                }

                var remaining = k_SetupTimeout - elapsed();
                if (remaining <= TimeSpan.Zero)
                {
                    var waitingFor = string.IsNullOrEmpty(clientId)
                        ? "its sign-in provider did not appear in Authentication"
                        : "the client did not show the new settings";
                    throw new InvalidOperationException(
                        $"Player Accounts accepted the client for project {client.ProjectId}, but {waitingFor} within {k_SetupTimeout.TotalSeconds:0} s. "
                        + "Run " + k_EnableCommand + " again: it is safe to repeat, and finishes the setup once the client is ready.");
                }

                await Delay(remaining < k_SetupPollInterval ? remaining : k_SetupPollInterval);
            }
        }

        internal static async Task<PlayerAccountsDto> GetAsync(IPlayerAccountsAdminClient client)
        {
            var signInProvider = await Call(client, () => client.GetUnitySignInProviderAsync(), Subject.Project, id: null);
            var clientId = signInProvider == null ? null : signInProvider.ClientId;
            if (string.IsNullOrEmpty(clientId))
            {
                return PlayerAccountsDto.WithoutClient(client.ProjectId, PlayerAccountsStatus.NotSetUp, signInProvider);
            }

            var saved = await FindClientAsync(client, clientId);
            return saved == null
                ? PlayerAccountsDto.WithoutClient(client.ProjectId, PlayerAccountsStatus.ClientMissing, signInProvider)
                : PlayerAccountsDto.From(client.ProjectId, clientId, saved, signInProvider);
        }

        internal static async Task<PlayerAccountsDto> UpdateAsync(IPlayerAccountsAdminClient client, PlayerAccountsClientRequest changes)
        {
            var (clientId, signInProvider) = await FindClientIdAsync(client);

            var response = await Call(client, () => client.UpdateClientAsync(clientId, changes), Subject.Client, clientId);
            return PlayerAccountsDto.From(client.ProjectId, clientId, response, signInProvider);
        }

        internal static async Task<IdentityProviderListDto> ListIdentityProvidersAsync(IPlayerAccountsAdminClient client)
        {
            var providers = await Call(client, () => client.ListIdentityProvidersAsync(), Subject.Project, id: null);
            var dtos = new List<IdentityProviderDto>(providers.Length);
            foreach (var provider in providers)
            {
                dtos.Add(IdentityProviderDto.From(provider));
            }

            return new IdentityProviderListDto
            {
                ProjectId = client.ProjectId,
                Count = dtos.Count,
                IdentityProviders = dtos,
            };
        }

        internal static async Task<IdentityProviderDto> CreateIdentityProviderAsync(IPlayerAccountsAdminClient client, CreatePlayerAccountsIdentityProviderRequest request)
        {
            var response = await Call(client, () => client.CreateIdentityProviderAsync(request), Subject.Project, id: null);
            if (response == null || string.IsNullOrEmpty(response.Id))
            {
                throw new InvalidOperationException(
                    "The Player Accounts service did not return the registered app's id, so it is unknown whether it was registered. "
                    + "Check with " + k_ListIdentityProvidersCommand + " before registering it again.");
            }

            return IdentityProviderDto.From(response);
        }

        internal static Task<IdentityProviderDto> EnableIdentityProviderAsync(IPlayerAccountsAdminClient client, string id)
        {
            return SetIdentityProviderDisabledAsync(client, RequiredIdentityProviderId(id), disabled: false);
        }

        internal static Task<IdentityProviderDto> DisableIdentityProviderAsync(IPlayerAccountsAdminClient client, string id, bool confirm)
        {
            var providerId = RequiredIdentityProviderId(id);
            if (!confirm)
            {
                throw new ArgumentException(
                    $"Refusing to disable identity provider '{providerId}': pass --confirm true to disable it. "
                    + "A game signing in through that app stops being able to at once; it does not fall back to Unity's shared app.");
            }

            return SetIdentityProviderDisabledAsync(client, providerId, disabled: true);
        }

        internal static async Task<IdentityProviderDeletedDto> DeleteIdentityProviderAsync(IPlayerAccountsAdminClient client, string id, bool confirm, bool dryRun)
        {
            var providerId = RequiredIdentityProviderId(id);
            if (!dryRun && !confirm)
            {
                throw new ArgumentException(
                    $"Refusing to delete identity provider '{providerId}': pass --confirm true to delete it, or --dry-run true to see which app it is. Sign-in through that app stops working for players. "
                    + "To stop using it without deleting it, run " + k_DisableIdentityProviderCommand + " instead.");
            }

            // Looked up first on a real delete too, so the result names the app that was deleted.
            var registered = await FindIdentityProviderAsync(client, providerId);
            if (!dryRun)
            {
                await Call(client, () => client.DeleteIdentityProviderAsync(providerId), Subject.IdentityProvider, providerId);
            }

            return new IdentityProviderDeletedDto
            {
                ProjectId = client.ProjectId,
                Id = providerId,
                Provider = registered.Provider,
                ProviderClientId = registered.ProviderClientId,
                DryRun = dryRun,
                Deleted = !dryRun,
            };
        }

        static async Task<IdentityProviderDto> SetIdentityProviderDisabledAsync(IPlayerAccountsAdminClient client, string providerId, bool disabled)
        {
            var request = new UpdatePlayerAccountsIdentityProviderRequest { Disabled = disabled };
            var response = await Call(client, () => client.UpdateIdentityProviderAsync(providerId, request), Subject.IdentityProvider, providerId);
            return IdentityProviderDto.From(response);
        }

        static async Task<PlayerAccountsIdentityProviderResponse> FindIdentityProviderAsync(IPlayerAccountsAdminClient client, string providerId)
        {
            var providers = await Call(client, () => client.ListIdentityProvidersAsync(), Subject.Project, id: null);
            foreach (var provider in providers)
            {
                if (provider != null && string.Equals(provider.Id, providerId, StringComparison.Ordinal))
                {
                    return provider;
                }
            }

            throw new ArgumentException(IdentityProviderNotFound(client.ProjectId, providerId));
        }

        /// <summary>The client, or null while the service does not have it yet.</summary>
        static async Task<PlayerAccountsClientResponse> FindClientAsync(IPlayerAccountsAdminClient client, string clientId)
        {
            try
            {
                return await client.GetClientAsync(clientId);
            }
            catch (PlayerAccountsAdminException e) when (e.Reason == PlayerAccountsAdminException.FailureReason.NotFound)
            {
                return null;
            }
            catch (PlayerAccountsAdminException e)
            {
                throw Explain(e, client.ProjectId, Subject.Client, clientId);
            }
        }

        /// <summary>Whether the saved client has every setting the request gave.</summary>
        internal static bool Reflects(PlayerAccountsClientResponse saved, PlayerAccountsClientRequest request)
        {
            if (request.Name != null && !string.Equals(saved.Name, request.Name, StringComparison.Ordinal))
            {
                return false;
            }

            if (request.Platforms != null && !SamePlatforms(saved.Platforms, request.Platforms))
            {
                return false;
            }

            return SameLink(saved.TermsOfServiceLink, request.TermsOfServiceLink)
                && SameLink(saved.PrivacyPolicyLink, request.PrivacyPolicyLink);
        }

        static bool SamePlatforms(string[] saved, string[] requested)
        {
            if (saved == null || saved.Length != requested.Length)
            {
                return false;
            }

            foreach (var platform in requested)
            {
                if (Array.IndexOf(saved, platform) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        static bool SameLink(string saved, string requested)
        {
            return requested == null || string.Equals(saved ?? "", requested, StringComparison.Ordinal);
        }

        static async Task<(string clientId, IdProviderResponse signInProvider)> FindClientIdAsync(IPlayerAccountsAdminClient client)
        {
            var signInProvider = await Call(client, () => client.GetUnitySignInProviderAsync(), Subject.Project, id: null);
            if (signInProvider == null || string.IsNullOrEmpty(signInProvider.ClientId))
            {
                throw new InvalidOperationException(
                    $"Unity Player Accounts is not turned on for project {client.ProjectId}. Run " + k_EnableCommand + " first, "
                    + "and check the Editor is linked to the right project in " + AuthenticationCliConstants.ServicesProjectSettings + ".");
            }

            return (signInProvider.ClientId, signInProvider);
        }

        internal static string RequiredName(string name)
        {
            var trimmed = Required("name", name, "the name players see when they sign in, for example --name \"My Game\"", AuthenticationCliConstants.PlayerAccountsMaxNameLength);
            if (trimmed.Length < AuthenticationCliConstants.PlayerAccountsMinNameLength)
            {
                throw new ArgumentException($"--name must be at least {AuthenticationCliConstants.PlayerAccountsMinNameLength} characters.");
            }

            return trimmed;
        }

        static string RequiredIdentityProviderId(string id)
        {
            return Required("id", id, "the registered app's id; " + k_ListIdentityProvidersCommand + " lists them");
        }

        static string Required(string option, string value, string what, int maxLength = int.MaxValue)
        {
            var trimmed = value?.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                throw new ArgumentException($"--{option} is required: pass {what}.");
            }

            if (trimmed.Length > maxLength)
            {
                throw new ArgumentException($"--{option} is too long: at most {maxLength} characters.");
            }

            return trimmed;
        }

        internal static string[] ParsePlatforms(string platforms)
        {
            var items = SplitList(platforms);
            if (items.Length == 0)
            {
                throw new ArgumentException("--platforms is required: pass one or more of " + AuthenticationCliConstants.PlayerAccountsPlatformList + ", for example --platforms mobile,desktop.");
            }

            var parsed = new List<string>();
            foreach (var item in items)
            {
                var platform = item.ToLowerInvariant();
                if (Array.IndexOf(AuthenticationCliConstants.PlayerAccountsPlatforms, platform) < 0)
                {
                    throw new ArgumentException($"--platforms: '{item}' is not a Player Accounts platform. Use " + AuthenticationCliConstants.PlayerAccountsPlatformList + ".");
                }

                if (!parsed.Contains(platform))
                {
                    parsed.Add(platform);
                }
            }

            return parsed.ToArray();
        }

        internal static string ParseProvider(string provider)
        {
            var value = Required("provider", provider, AuthenticationCliConstants.PlayerAccountsIdentityProviderList).ToLowerInvariant();
            if (Array.IndexOf(AuthenticationCliConstants.PlayerAccountsIdentityProviders, value) < 0)
            {
                throw new ArgumentException($"--provider '{provider}' is not supported. Use " + AuthenticationCliConstants.PlayerAccountsIdentityProviderList + ".");
            }

            return value;
        }

        /// <summary>
        /// Null when the option was not given. An empty value is refused rather than read as "remove", so an unset
        /// variable passed as the link cannot delete it.
        /// </summary>
        internal static string OptionalUrl(string option, string url)
        {
            if (url == null)
            {
                return null;
            }

            var trimmed = url.Trim();
            if (trimmed.Length == 0)
            {
                throw new ArgumentException(
                    $"--{option} was given but is empty. Pass a full link, or leave the option out. To remove the current link, run " + k_UpdateCommand + $" --remove-{option} true.");
            }

            if (trimmed.Length > AuthenticationCliConstants.PlayerAccountsMaxLinkLength)
            {
                throw new ArgumentException($"--{option} is too long: at most {AuthenticationCliConstants.PlayerAccountsMaxLinkLength} characters.");
            }

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                throw new ArgumentException($"--{option}: '{trimmed}' is not a web address. Pass a full link, for example https://example.com/{option.Replace("-url", "")}.");
            }

            return trimmed;
        }

        /// <summary>
        /// The link to send on an update: null to keep it, empty to remove it (only when <paramref name="remove"/> is
        /// set), or the new link.
        /// </summary>
        internal static string LinkChange(string option, string url, bool remove)
        {
            if (!remove)
            {
                return OptionalUrl(option, url);
            }

            if (url != null)
            {
                throw new ArgumentException($"Pass either --{option} or --remove-{option} true, not both.");
            }

            return "";
        }

        static string[] SplitList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            var items = new List<string>();
            foreach (var part in value.Split(','))
            {
                var item = part.Trim();
                if (item.Length > 0)
                {
                    items.Add(item);
                }
            }

            return items.ToArray();
        }

        static IPlayerAccountsAdminClient CreateClient()
        {
            var client = AuthenticationAdminClientManager.CreatePlayerAccountsAdminClient();
            if (client == null)
            {
                throw new InvalidOperationException(
                    "This project is not linked to a Unity Cloud project, and Player Accounts belongs to one. "
                    + "Link it in " + AuthenticationCliConstants.ServicesProjectSettings + " while signed in to the Editor, then run this command again.");
            }

            return client;
        }

        static async Task<T> Call<T>(IPlayerAccountsAdminClient client, Func<Task<T>> call, Subject subject, string id)
        {
            try
            {
                return await call();
            }
            catch (PlayerAccountsAdminException e)
            {
                throw Explain(e, client.ProjectId, subject, id);
            }
        }

        static async Task Call(IPlayerAccountsAdminClient client, Func<Task> call, Subject subject, string id)
        {
            try
            {
                await call();
            }
            catch (PlayerAccountsAdminException e)
            {
                throw Explain(e, client.ProjectId, subject, id);
            }
        }

        static string IdentityProviderNotFound(string projectId, string id)
        {
            return $"Project {projectId} has no registered social provider app with id '{id}'. "
                + "Run " + k_ListIdentityProvidersCommand + " for the registered apps' ids, and check the Editor is linked to the right project in " + AuthenticationCliConstants.ServicesProjectSettings + ".";
        }

        /// <summary>
        /// Turns a failed call into a message that names what to fix. Never includes request or response bodies beyond
        /// the server's own problem detail.
        /// </summary>
        internal static Exception Explain(PlayerAccountsAdminException exception, string projectId, Subject subject, string id)
        {
            var detail = string.IsNullOrEmpty(exception.ServerDetail) ? "" : $" (server: {exception.ServerDetail})";
            switch (exception.Reason)
            {
                case PlayerAccountsAdminException.FailureReason.Unauthorized:
                    return new InvalidOperationException(
                        "The Editor's Unity sign-in is missing or has expired, so the request was refused. "
                        + "Sign in again from the account menu in the Editor's toolbar, then run this command again." + detail, exception);
                case PlayerAccountsAdminException.FailureReason.Forbidden:
                    return new InvalidOperationException(
                        $"The signed-in Unity account is not allowed to manage Player Accounts in project {projectId}. "
                        + "Ask an organization Owner or Manager to give your account the " + AuthenticationCliConstants.AuthenticationEditorRole
                        + " role (under " + AuthenticationCliConstants.AuthenticationRolesCategory + ") in " + AuthenticationCliConstants.MembersDashboardPath
                        + ", or sign in to the Editor with an account that has it. " + AuthenticationCliConstants.AuthenticationViewerRole
                        + " is enough for " + k_GetCommand + " and " + k_ListIdentityProvidersCommand + "." + detail, exception);
                case PlayerAccountsAdminException.FailureReason.NotFound when subject == Subject.Client:
                    return new InvalidOperationException(
                        $"Player Accounts has no client with id '{id}' in project {projectId}, though the project's Unity Player Accounts sign-in provider in Authentication points at it. "
                        + "Run " + k_EnableCommand + " to set the client up again." + detail, exception);
                case PlayerAccountsAdminException.FailureReason.NotFound when subject == Subject.IdentityProvider:
                    return new ArgumentException(IdentityProviderNotFound(projectId, id) + detail, exception);
                case PlayerAccountsAdminException.FailureReason.NotFound:
                    return new InvalidOperationException(
                        $"The Player Accounts service does not know project {projectId}. Check the Editor is linked to the right project in "
                        + AuthenticationCliConstants.ServicesProjectSettings + ", or run " + k_EnableCommand + " to turn Player Accounts on for it." + detail, exception);
                case PlayerAccountsAdminException.FailureReason.InvalidRequest:
                    return new ArgumentException(
                        "The Player Accounts service rejected the request. Correct the argument the server names and run the command again." + detail, exception);
                case PlayerAccountsAdminException.FailureReason.Network:
                    return new InvalidOperationException(
                        "Could not reach the Player Accounts service. Check the network connection, then run this command again." + detail, exception);
                default:
                    return new InvalidOperationException(
                        $"The Player Accounts service failed the request (HTTP {exception.StatusCode}). Run the command again; if it keeps failing, the service may be unavailable." + detail, exception);
            }
        }

        /// <summary>Where the project's Player Accounts setup stands.</summary>
        public enum PlayerAccountsStatus
        {
            Unknown = 0,
            /// <summary>The project has no Unity Player Accounts sign-in provider in Authentication.</summary>
            NotSetUp = 10,
            /// <summary>The sign-in provider exists, but Player Accounts has no client under its id. <c>enable</c> sets one up.</summary>
            ClientMissing = 20,
            /// <summary>The sign-in provider points at the project's Player Accounts client.</summary>
            Ready = 30,
        }

        /// <summary>The project's Player Accounts setup as the commands report it.</summary>
        public class PlayerAccountsDto
        {
            public string ProjectId { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public PlayerAccountsStatus Status { get; set; }
            /// <summary>The Client ID to set in Services > Unity Player Accounts. Null unless <see cref="Status"/> is Ready.</summary>
            public string ClientId { get; set; }
            public string Name { get; set; }
            public string[] Platforms { get; set; }
            /// <summary>The terms of service link, as --terms-url sets it.</summary>
            public string TermsUrl { get; set; }
            /// <summary>The privacy policy link, as --privacy-url sets it.</summary>
            public string PrivacyUrl { get; set; }
            /// <summary>Whether Player Accounts sign-in is enabled in Authentication.</summary>
            public bool SignInEnabled { get; set; }

            internal static PlayerAccountsDto From(string projectId, string clientId, PlayerAccountsClientResponse response, IdProviderResponse signInProvider)
            {
                return new PlayerAccountsDto
                {
                    ProjectId = projectId,
                    Status = PlayerAccountsStatus.Ready,
                    ClientId = clientId,
                    Name = response?.Name,
                    Platforms = response?.Platforms ?? Array.Empty<string>(),
                    TermsUrl = NullIfEmpty(response?.TermsOfServiceLink),
                    PrivacyUrl = NullIfEmpty(response?.PrivacyPolicyLink),
                    SignInEnabled = signInProvider != null && !signInProvider.Disabled,
                };
            }

            // ClientMissing leaves ClientId empty on purpose: the provider's id names no client, and an older setup
            // holds the project id there, which must not be pasted into Services > Unity Player Accounts.
            internal static PlayerAccountsDto WithoutClient(string projectId, PlayerAccountsStatus status, IdProviderResponse signInProvider)
            {
                return new PlayerAccountsDto
                {
                    ProjectId = projectId,
                    Status = status,
                    Platforms = Array.Empty<string>(),
                    SignInEnabled = signInProvider != null && !signInProvider.Disabled,
                };
            }
        }

        public class IdentityProviderDto
        {
            public string Id { get; set; }
            public string Provider { get; set; }
            /// <summary>The app's client id from the provider's developer console, not the Player Accounts Client ID.</summary>
            public string ProviderClientId { get; set; }
            public bool Enabled { get; set; }

            internal static IdentityProviderDto From(PlayerAccountsIdentityProviderResponse response)
            {
                return new IdentityProviderDto
                {
                    Id = response?.Id,
                    Provider = response?.Provider,
                    ProviderClientId = response?.ProviderClientId,
                    Enabled = response != null && !response.Disabled,
                };
            }
        }

        static string NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

        public class IdentityProviderListDto
        {
            public string ProjectId { get; set; }
            public int Count { get; set; }
            public List<IdentityProviderDto> IdentityProviders { get; set; }
        }

        public class IdentityProviderDeletedDto
        {
            public string ProjectId { get; set; }
            public string Id { get; set; }
            /// <summary>The app's provider.</summary>
            public string Provider { get; set; }
            /// <summary>The app's client id from the provider's developer console, not the Player Accounts Client ID.</summary>
            public string ProviderClientId { get; set; }
            public bool DryRun { get; set; }
            public bool Deleted { get; set; }
        }
    }
}
#endif
