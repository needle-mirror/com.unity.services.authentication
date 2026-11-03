#if UNITY_PIPELINE_AVAILABLE && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Pipeline.Commands;
using Unity.Services.Authentication.Components;
using Unity.Services.Core.Components;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Services.Authentication.Cli
{
    static class PlayerAuthenticationEditorCliCommands
    {
        [CliGroupCommand(AuthenticationCliConstants.Group,
            "Add the Player Authentication component to a GameObject in the open scene, so the player signs in in Play Mode without any game code. Creates the GameObject when it does not exist, and reports whether a Services Initialization component will initialize the services for it. Re-running targets the component already in the scene and rewrites it from the arguments given. Refused in Play Mode. The scene is left unsaved: save it with " + AuthenticationCliConstants.SceneGroup + " save")]
        public static PlayerAuthenticationComponentDto AddPlayerAuthenticationComponent(
            [CliArg("game-object", "Name or hierarchy path (Parent/Child) of the GameObject to host the component. Created in the active scene when nothing matches a plain name. Defaults to the GameObject already holding the component, else 'Unity Services', which is where " + AuthenticationCliConstants.CoreGroup + " add_services_initialization_component puts its own component.")] string gameObject = null,
            [CliArg("sign-in-anonymously", "Sign in anonymously once the services are initialized. Pass false to sign in from your own code instead.")] bool signInAnonymously = true,
            [CliArg("fetch-player-info", "Fetch the player info on sign in: creation time, linked identities, username.")] bool fetchPlayerInfo = false,
            [CliArg("fetch-player-name", "Fetch the player name on sign in.")] bool fetchPlayerName = false,
            [CliArg("generate-name", "Generate a player name when the player has none. Requires fetch-player-name.")] bool generateName = false,
            [CliArg("profile", "Profile to scope the persisted credentials to, which is how one machine signs in as several players: 1 to 30 letters, digits, '-' or '_'. Omit to leave the component's profile untouched; pass an empty string to go back to the default profile.")] string profile = null)
        {
            RefusePlayMode();
            RefuseNameGenerationWithoutFetch(fetchPlayerName, generateName);
            var customProfile = ValidatedProfile(profile);

            var existing = FindSingleInOpenScenes();
            var target = NormalizeTarget(gameObject);
            RefuseComponentOnAnotherGameObject(existing, target);

            var undoGroup = BeginUndoGroup(AuthenticationCliConstants.PlayerAuthenticationUndoName);
            try
            {
                var component = existing ?? AddToScene(target ?? AuthenticationCliConstants.DefaultGameObjectName);
                Configure(component, signInAnonymously, fetchPlayerInfo, fetchPlayerName, generateName, customProfile);
                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                Undo.CollapseUndoOperations(undoGroup);
                return Describe(component, created: existing == null);
            }
            catch
            {
                Undo.RevertAllDownToGroup(undoGroup);
                throw;
            }
        }

        static void RefusePlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "The Editor is in Play Mode, and scene changes made in Play Mode are lost when it exits. "
                    + "Exit Play Mode with " + AuthenticationCliConstants.PlayModeGroup + " stop, then run this command again.");
            }
        }

        static void RefuseNameGenerationWithoutFetch(bool fetchPlayerName, bool generateName)
        {
            if (generateName && !fetchPlayerName)
            {
                throw new ArgumentException(
                    "--generate-name requires --fetch-player-name: the name is only generated on the fetch. "
                    + "Pass --fetch-player-name true as well, or drop --generate-name.");
            }
        }

        static string ValidatedProfile(string profile)
        {
            var custom = profile?.Trim();
            if (!string.IsNullOrEmpty(custom) && !Regex.IsMatch(custom, AuthenticationCliConstants.ProfilePattern))
            {
                throw new ArgumentException(
                    $"--profile \"{custom}\" is not a valid profile name, and the player would never sign in: switching to it throws in Play Mode. "
                    + "Use 1 to 30 letters, digits, '-' or '_', for example \"player_one\". "
                    + "Pass an empty string to go back to the default profile. Nothing was changed.");
            }

            return custom;
        }

        static string NormalizeTarget(string gameObject)
        {
            var target = gameObject?.Replace('\\', '/').Trim().Trim('/');
            return string.IsNullOrEmpty(target) ? null : target;
        }

        static PlayerAuthentication FindSingleInOpenScenes()
        {
            var components = FindAllInOpenScenes<PlayerAuthentication>();
            if (components.Length > 1)
            {
                var hosts = new GameObject[components.Length];
                for (var i = 0; i < components.Length; i++)
                {
                    hosts[i] = components[i].gameObject;
                }

                throw new InvalidOperationException(
                    $"The open scenes hold {components.Length} Player Authentication components ({Describe(hosts)}), "
                    + "and signing in twice is a defect. Remove all but one with " + AuthenticationCliConstants.ComponentGroup + " remove, then run this command again.");
            }

            return components.Length == 1 ? components[0] : null;
        }

        static T[] FindAllInOpenScenes<T>() where T : Object
        {
#if UNITY_6000_6_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include);
#else
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#endif
        }

        static void RefuseComponentOnAnotherGameObject(PlayerAuthentication existing, string target)
        {
            if (existing == null || target == null || Matches(existing.gameObject, target))
            {
                return;
            }

            throw new InvalidOperationException(
                $"A Player Authentication component is already on '{HierarchyPath(existing.gameObject)}', and signing in twice is a defect. "
                + $"Either omit --game-object or pass --game-object \"{HierarchyPath(existing.gameObject)}\" to update it, "
                + "or remove it with " + AuthenticationCliConstants.ComponentGroup + " remove first.");
        }

        static PlayerAuthentication AddToScene(string target)
        {
            var host = FindOrCreateGameObject(target);
            var component = Undo.AddComponent<PlayerAuthentication>(host);
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Unity did not add the Player Authentication component to '{HierarchyPath(host)}'. The Console holds the reason.");
            }

            return component;
        }

        static GameObject FindOrCreateGameObject(string target)
        {
            var matches = new List<GameObject>();
            foreach (var transform in FindAllInOpenScenes<Transform>())
            {
                if (Matches(transform.gameObject, target))
                {
                    matches.Add(transform.gameObject);
                }
            }

            if (matches.Count > 1)
            {
                throw new ArgumentException(
                    $"--game-object \"{target}\" matches {matches.Count} GameObjects ({Describe(matches)}). "
                    + "Pass the hierarchy path of one of them; " + AuthenticationCliConstants.SceneGroup + " get_hierarchy lists the paths.");
            }

            if (matches.Count == 1)
            {
                return matches[0];
            }

            if (target.Contains("/"))
            {
                throw new ArgumentException(
                    $"No GameObject in the open scenes is at '{target}'. Check the path with " + AuthenticationCliConstants.SceneGroup + " get_hierarchy, "
                    + "or pass a plain name to create a root GameObject.");
            }

            var host = new GameObject(target);
            Undo.RegisterCreatedObjectUndo(host, AuthenticationCliConstants.PlayerAuthenticationUndoName);
            return host;
        }

        static void Configure(
            PlayerAuthentication component,
            bool signInAnonymously,
            bool fetchPlayerInfo,
            bool fetchPlayerName,
            bool generateName,
            string customProfile)
        {
            var serialized = new SerializedObject(component);
            RequiredProperty(serialized, nameof(PlayerAuthentication.SignInAnonymously)).boolValue = signInAnonymously;
            RequiredProperty(serialized, nameof(PlayerAuthentication.FetchPlayerInfo)).boolValue = fetchPlayerInfo;
            RequiredProperty(serialized, nameof(PlayerAuthentication.FetchPlayerName)).boolValue = fetchPlayerName;
            RequiredProperty(serialized, nameof(PlayerAuthentication.GenerateName)).boolValue = generateName;
            ConfigureProfile(serialized, customProfile);
            serialized.ApplyModifiedProperties();
        }

        static void ConfigureProfile(SerializedObject serialized, string customProfile)
        {
            if (customProfile == null)
            {
                return;
            }

            RequiredProperty(serialized, nameof(PlayerAuthentication.SetCustomProfile)).boolValue = customProfile.Length > 0;
            if (customProfile.Length > 0)
            {
                RequiredProperty(serialized, nameof(PlayerAuthentication.Profile)).stringValue = customProfile;
            }
        }

        static SerializedProperty RequiredProperty(SerializedObject serialized, string name)
        {
            var property = serialized.FindProperty(name);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"The Player Authentication component has no serialized field '{name}', so this command does not match "
                    + "the installed com.unity.services.authentication. Nothing was changed. Update com.unity.services.authentication, "
                    + "or set the field in the Inspector instead.");
            }

            return property;
        }

        static bool Matches(GameObject candidate, string target)
        {
            return string.Equals(candidate.name, target, StringComparison.Ordinal)
                || string.Equals(HierarchyPath(candidate), target, StringComparison.Ordinal);
        }

        static int BeginUndoGroup(string name)
        {
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(name);
            return group;
        }

        static string HierarchyPath(GameObject gameObject)
        {
            return SearchUtils.GetHierarchyPath(gameObject, false).TrimStart('/');
        }

        static string Describe(IReadOnlyList<GameObject> gameObjects)
        {
            var descriptions = new string[gameObjects.Count];
            for (var i = 0; i < gameObjects.Count; i++)
            {
                descriptions[i] = $"'{gameObjects[i].scene.name}:{HierarchyPath(gameObjects[i])}'";
            }

            return string.Join(", ", descriptions);
        }

        static bool SharesRegistry(ServicesBehaviour a, ServicesBehaviour b)
        {
            return a.UseCustomServices == b.UseCustomServices
                && (!a.UseCustomServices || string.Equals(a.ServicesIdentifier, b.ServicesIdentifier, StringComparison.Ordinal));
        }

        static bool InitializesOnStart(ServicesInitialization initializer, PlayerAuthentication component)
        {
            return initializer.isActiveAndEnabled && initializer.InitializeOnStart && SharesRegistry(initializer, component);
        }

        static ServicesInitialization FindRelevantInitializer(PlayerAuthentication component)
        {
            ServicesInitialization sameRegistry = null;
            ServicesInitialization any = null;
            foreach (var initializer in FindAllInOpenScenes<ServicesInitialization>())
            {
                if (InitializesOnStart(initializer, component))
                {
                    return initializer;
                }

                if (sameRegistry == null && SharesRegistry(initializer, component))
                {
                    sameRegistry = initializer;
                }

                any = any ?? initializer;
            }

            return sameRegistry ?? any;
        }

        static string InitializationProblem(PlayerAuthentication component, ServicesInitialization initializer)
        {
            if (!component.isActiveAndEnabled)
            {
                return $"The Player Authentication component on '{HierarchyPath(component.gameObject)}' is disabled or on an inactive GameObject, "
                    + "so it never signs anyone in. Enable the component and activate the GameObject.";
            }

            if (initializer == null)
            {
                return "Nothing in the open scenes initializes the services, so this component will not sign anyone in "
                    + "unless your own code calls UnityServices.InitializeAsync. Otherwise run " + AuthenticationCliConstants.CoreGroup + " add_services_initialization_component.";
            }

            if (InitializesOnStart(initializer, component))
            {
                return null;
            }

            var where = $"The Services Initialization component on '{HierarchyPath(initializer.gameObject)}'";
            if (!SharesRegistry(initializer, component))
            {
                return $"{where} initializes a different services registry than this component uses (Use Custom Services / Services Identifier differ), "
                    + "so the player is not signed in. Set both components to the same registry in the Inspector.";
            }

            if (!initializer.isActiveAndEnabled)
            {
                return $"{where} is disabled or on an inactive GameObject, so the services never initialize and the player is not signed in. "
                    + "Enable the component and activate the GameObject.";
            }

            return $"{where} has initialize-on-start off, so the player signs in only once your own code calls UnityServices.InitializeAsync. "
                + "To initialize automatically, run " + AuthenticationCliConstants.CoreGroup + " add_services_initialization_component --initialize-on-start true.";
        }

        static PlayerAuthenticationComponentDto Describe(PlayerAuthentication component, bool created)
        {
            var initializer = FindRelevantInitializer(component);

            return new PlayerAuthenticationComponentDto
            {
                GameObject = HierarchyPath(component.gameObject),
                Scene = component.gameObject.scene.path,
                Created = created,
                SignInAnonymously = component.SignInAnonymously,
                FetchPlayerInfo = component.FetchPlayerInfo,
                FetchPlayerName = component.FetchPlayerName,
                GenerateName = component.GenerateName,
                Profile = component.SetCustomProfile ? component.Profile : null,
                ServicesInitialization = initializer == null ? null : HierarchyPath(initializer.gameObject),
                ServicesInitializeOnStart = initializer != null && InitializesOnStart(initializer, component),
                Message = InitializationProblem(component, initializer)
            };
        }

        public class PlayerAuthenticationComponentDto
        {
            public string GameObject { get; set; }
            public string Scene { get; set; }
            public bool Created { get; set; }
            public bool SignInAnonymously { get; set; }
            public bool FetchPlayerInfo { get; set; }
            public bool FetchPlayerName { get; set; }
            public bool GenerateName { get; set; }
            public string Profile { get; set; }
            public string ServicesInitialization { get; set; }
            public bool ServicesInitializeOnStart { get; set; }
            public string Message { get; set; }
        }
    }
}
#endif
