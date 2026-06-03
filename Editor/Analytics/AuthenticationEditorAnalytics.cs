using System;
using Unity.Services.Core.Editor;
using UnityEditor;
#if UNITY_2023_2_OR_NEWER
using UnityEngine.Analytics;
#endif

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

        const string k_EventName = "editorgameserviceeditor";
        const int k_Version = 1;
#if UNITY_2023_2_OR_NEWER
        const string k_VendorKey = "unity.services.authentication";
#endif

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

#if UNITY_2023_2_OR_NEWER
        [AnalyticInfo(eventName: k_EventName, vendorKey: k_VendorKey, version: k_Version)]
        class AuthenticationAnalyticEvent : IAnalytic
        {
            readonly EditorGameServiceEvent m_Event;

            public AuthenticationAnalyticEvent(EditorGameServiceEvent evt)
            {
                m_Event = evt;
            }

            public bool TryGatherData(out IAnalytic.IData data, out Exception error)
            {
                error = null;
                data = m_Event;
                return data != null;
            }
        }
#endif

        /// <remarks>Lowercase is used here for compatibility with analytics.</remarks>
        [Serializable]
        struct EditorGameServiceEvent
#if UNITY_2023_2_OR_NEWER
            : IAnalytic.IData
#endif
        {
            public string action;
            public string component;
            public string package;
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
#if UNITY_2023_2_OR_NEWER
            EditorAnalytics.SendAnalytic(new AuthenticationAnalyticEvent(new EditorGameServiceEvent
            {
                action = action,
                component = component,
                package = Identifier.GetKey()
            }));
#else
            EditorAnalytics.SendEventWithLimit(k_EventName, new EditorGameServiceEvent
            {
                action = action,
                component = component,
                package = Identifier.GetKey()
            }, k_Version);
#endif
        }
    }
}
