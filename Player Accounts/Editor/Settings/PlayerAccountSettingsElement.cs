using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    class PlayerAccountSettingsElement : VisualElement
    {
        VisualElement Panel { get; }

        internal const string UssPath = "Packages/com.unity.services.authentication/Player Accounts/Editor/Settings/PlayerAccountStyle.uss";

        readonly UnityPlayerAccountSettings m_Settings;

        public PlayerAccountSettingsElement(UnityPlayerAccountSettings settings)
        {
            m_Settings = settings;
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath));
            Panel = new VisualElement();
            Panel.AddToClassList("player-accounts-panel");

            CreateClientIdField();
            CreateScopeField();
            CreateDeepLinkFields();
        }

        void CreateClientIdField()
        {
            var clientIdField = new TextField("Client ID");
            clientIdField.isDelayed = true;
            clientIdField.SetValueWithoutNotify(m_Settings.ClientId);
            clientIdField.RegisterValueChangedCallback(e =>
            {
                m_Settings.ClientId = e.newValue;
                Save();
            });

            Panel.Add(clientIdField);
        }

        void CreateScopeField()
        {
            var scopeField = new MaskField("Scope", Enum.GetNames(typeof(UnityPlayerAccountSettings.SupportedScopesEnum)).ToList(), (int)m_Settings.ScopeFlags);
            scopeField.RegisterValueChangedCallback(e =>
            {
                m_Settings.ScopeFlags = (UnityPlayerAccountSettings.SupportedScopesEnum)e.newValue;
                Save();
            });

            Panel.Add(scopeField);
        }

        void CreateDeepLinkFields()
        {
            var useCustomDeepLinkUriToggle = new Toggle("Use Custom Deep Link Uri");
            useCustomDeepLinkUriToggle.value = m_Settings.useCustomDeepLinkUri;

            var customSchemeField = new TextField("Custom Scheme");
            customSchemeField.isDelayed = true;
            customSchemeField.value = m_Settings.customScheme;
            customSchemeField.visible = useCustomDeepLinkUriToggle.value;

            var customHostField = new TextField("Custom Host");
            customHostField.isDelayed = true;
            customHostField.value = m_Settings.customHost;
            customHostField.visible = useCustomDeepLinkUriToggle.value;

            useCustomDeepLinkUriToggle.RegisterValueChangedCallback(evt =>
            {
                customSchemeField.visible = evt.newValue;
                customHostField.visible = evt.newValue;
                Save();
            });

            customSchemeField.RegisterValueChangedCallback(evt =>
            {
                m_Settings.customScheme = evt.newValue;
                Save();
            });

            customHostField.RegisterValueChangedCallback(evt =>
            {
                m_Settings.customHost = evt.newValue;
                Save();
            });

            Panel.Add(useCustomDeepLinkUriToggle);
            Panel.Add(customSchemeField);
            Panel.Add(customHostField);
            Add(Panel);
        }

        void Save()
        {
            EditorUtility.SetDirty(m_Settings);
            AssetDatabase.SaveAssetIfDirty(m_Settings);
        }
    }
}
