using System;
using Unity.Services.Core.Editor;
using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    static class AuthenticationEditorAnalytics
    {
        static class Component
        {
            public const string ProjectSettings = "Project Settings";
            public const string TopMenu = "Top Menu";
        }

        static class Action
        {
            public const string Configure = "Configure";
            public const string Tool = "Tool";
            public const string AddProvider = "Add Provider";
            public const string SaveProvider = "Save Provider";
            public const string Refresh = "Refresh";
            public const string NetworkError = "Network Error";
        }

        const int k_Version = 1;
        const string k_EventName = "editorgameserviceeditor";

        static IEditorGameServiceIdentifier s_Identifier;

        static IEditorGameServiceIdentifier Identifier
        {
            get
            {
                if (s_Identifier == null)
                {
                    s_Identifier = EditorGameServiceRegistry.Instance.GetEditorGameService<AuthenticationIdentifier>().Identifier;
                }
                return s_Identifier;
            }
        }

        internal static void SendProjectSettingsToolEvent()
        {
            SendEvent(Component.ProjectSettings, Action.Tool);
        }

        internal static void SendTopMenuConfigureEvent()
        {
            SendEvent(Component.TopMenu, Action.Configure);
        }

        internal static void SendAddProviderEvent()
        {
            SendEvent(Component.ProjectSettings, Action.AddProvider);
        }

        internal static void SendSaveProviderEvent()
        {
            SendEvent(Component.ProjectSettings, Action.SaveProvider);
        }

        internal static void SendRefreshEvent()
        {
            SendEvent(Component.ProjectSettings, Action.Refresh);
        }

        internal static void SendErrorEvent()
        {
            SendEvent(Component.ProjectSettings, Action.NetworkError);
        }

        static void SendEvent(string component, string action)
        {
            EditorAnalytics.SendEventWithLimit(k_EventName, new EditorGameServiceEvent
            {
                action = action,
                component = component,
                package = Identifier.GetKey()
            }, k_Version);
        }

        /// <remarks>Lowercase is used here for compatibility with analytics.</remarks>
        [Serializable]
        struct EditorGameServiceEvent
        {
            public string action;
            public string component;
            public string package;
        }
    }
}
