#if UNITY_PIPELINE_AVAILABLE
using System.Linq;
using NUnit.Framework;
using Unity.Pipeline.Commands;

namespace Unity.Services.Authentication.Cli.Tests
{
    class AuthenticationCliRegistrationTests
    {
        const string k_CliAssembly = "Unity.Services.Authentication.Cli";
        const string k_PlayerAccountsGroup = "services.authentication.player_accounts";

        [Test]
        public void DiscoverCommands_RegistersThePlayerAccountsCommands()
        {
            var registered = CommandRegistry.DiscoverCommands()
                .Where(command => command.Package == k_CliAssembly)
                .Select(command => (command.Group, command.Name))
                .ToArray();

            Assert.That(registered, Does.Contain((k_PlayerAccountsGroup, "enable")));
            Assert.That(registered, Does.Contain((k_PlayerAccountsGroup, "get")));
            Assert.That(registered, Does.Contain((k_PlayerAccountsGroup, "update")));
            Assert.That(registered, Does.Contain((k_PlayerAccountsGroup, "list_identity_providers")));
            Assert.That(registered, Does.Contain((k_PlayerAccountsGroup, "create_identity_provider")));
            Assert.That(registered, Does.Contain((k_PlayerAccountsGroup, "enable_identity_provider")));
            Assert.That(registered, Does.Contain((k_PlayerAccountsGroup, "disable_identity_provider")));
            Assert.That(registered, Does.Contain((k_PlayerAccountsGroup, "delete_identity_provider")));
        }

        [TestCase("enable", new[] { "name", "platforms" })]
        [TestCase("update", new string[0])]
        [TestCase("create_identity_provider", new[] { "provider", "provider-client-id" })]
        [TestCase("enable_identity_provider", new[] { "id" })]
        [TestCase("disable_identity_provider", new[] { "id" })]
        [TestCase("delete_identity_provider", new[] { "id" })]
        public void DiscoverCommands_MarksOnlyTheArgumentsAPlayerAccountsCommandNeedsAsRequired(string command, string[] required)
        {
            var registered = CommandRegistry.DiscoverCommands()
                .Single(info => info.Package == k_CliAssembly && info.Group == k_PlayerAccountsGroup && info.Name == command);

            var actual = registered.Parameters.Where(parameter => parameter.Required).Select(parameter => parameter.Name);

            Assert.That(actual, Is.EquivalentTo(required));
        }
    }
}
#endif
