using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication.Editor.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Logger = Unity.Services.Authentication.Utilities.Logger;

namespace Unity.Services.Authentication.Editor
{
    class AuthenticationSettingsElement : VisualElement
    {
        const string k_Uxml = "Packages/com.unity.services.authentication/Editor/UXML/AuthenticationProjectSettings.uxml";
        const string k_Uss = "Packages/com.unity.services.authentication/Editor/USS/AuthenticationStyleSheet.uss";

        IAuthenticationAdminClient m_AdminClient;

        string m_ProjectId;
        string m_IdDomainId;

        // Whether skip the confirmation window for tests/automation.
        bool m_SkipConfirmation;

        TextElement m_WaitingTextElement;
        TextElement m_ErrorTextElement;
        VisualElement m_AddIdProviderContainer;
        List<string> m_AddIdProviderTypeChoices;
        PopupField<string> m_AddIdProviderType;
        Button m_RefreshButton;
        Button m_AddButton;
        VisualElement m_IdProviderListContainer;

        /// <summary>
        /// The text to show when the settings is waiting for a task to finish.
        /// </summary>
        public TextElement WaitingTextElement => m_WaitingTextElement;

        /// <summary>
        /// The text to show when there is an error.
        /// </summary>
        public TextElement ErrorTextElement => m_ErrorTextElement;

        /// <summary>
        /// The add ID provider choices in the dropdown list.
        /// </summary>
        public IEnumerable<string> AddIdProviderTypeChoices => m_AddIdProviderTypeChoices;

        /// <summary>
        /// The add ID provider dropdown list.
        /// </summary>
        public PopupField<string> AddIdProviderType => m_AddIdProviderType;

        /// <summary>
        /// The button to refresh the ID provider list.
        /// </summary>
        public Button RefreshButton => m_RefreshButton;

        /// <summary>
        /// The button to add a new ID provider.
        /// </summary>
        public Button AddButton => m_AddButton;

        /// <summary>
        /// The container to add ID providers.
        /// </summary>
        public VisualElement IdProviderListContainer => m_IdProviderListContainer;

        /// <summary>
        /// Event triggered when the <cref="AuthenticationSettingsElement"/> starts or finishes waiting for a task.
        /// The first parameter of the callback is the sender.
        /// The second parameter is true if it starts waiting, and false if it finishes waiting.
        /// </summary>
        public event Action<AuthenticationSettingsElement, bool> Waiting;

        public AuthenticationSettingsElement(IAuthenticationAdminClient adminClient, string projectId, bool skipConfirmation = false)
        {
            m_AdminClient = adminClient;
            m_ProjectId = projectId;
            m_SkipConfirmation = skipConfirmation;

            var containerAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_Uxml);
            if (containerAsset != null)
            {
                var containerUI = containerAsset.CloneTree().contentContainer;

                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_Uss);
                if (styleSheet != null)
                {
                    containerUI.styleSheets.Add(styleSheet);
                }
                else
                {
                    throw new Exception("Asset not found: " + k_Uss);
                }

                m_WaitingTextElement = containerUI.Q<TextElement>(className: "auth-progress");
                m_ErrorTextElement = containerUI.Q<TextElement>(className: "auth-error");

                m_RefreshButton = containerUI.Q<Button>("id-provider-refresh");
                m_RefreshButton.clicked += RefreshIdProviders;

                m_AddButton = containerUI.Q<Button>("id-provider-add");
                m_AddButton.SetEnabled(false);
                m_AddButton.clicked += AddIdProvider;

                m_IdProviderListContainer = containerUI.Q<VisualElement>(className: "auth-id-provider-list");

                m_AddIdProviderContainer = containerUI.Q<VisualElement>("id-provider-type");

                Add(containerUI);
            }
            else
            {
                throw new Exception("Asset not found: " + k_Uxml);
            }
        }

        public async void RefreshIdProviders()
        {
            ShowWaiting();

            try
            {
                if (m_IdDomainId == null)
                {
                    await GetIdDomainAsync();
                }

                await ListIdProvidersAsync();
            }
            catch (Exception e)
            {
                OnError(e);
            }

            HideWaiting();
        }

        async Task GetIdDomainAsync()
        {
            m_IdDomainId = await m_AdminClient.GetIDDomainAsync();
        }

        async Task ListIdProvidersAsync()
        {
            m_IdProviderListContainer.Clear();
            var response = await m_AdminClient.ListIdProvidersAsync(m_IdDomainId);

            if (response.Results != null)
            {
                foreach (var provider in response.Results)
                {
                    CreateIdProviderElement(provider);
                }
            }

            UpdateAddIdproviderList();
        }

        void UpdateAddIdproviderList()
        {
            var unusedIdProviders = new List<string>(IdProviderRegistry.AllTypes);

            foreach (var child in m_IdProviderListContainer.Children())
            {
                if (!(child is IdProviderElement))
                {
                    continue;
                }

                var idProviderElement = (IdProviderElement)child;
                unusedIdProviders.Remove(idProviderElement.SavedValue.Type);
            }

            unusedIdProviders.Sort();

            m_AddIdProviderContainer.Clear();
            m_AddIdProviderTypeChoices = unusedIdProviders;

            if (unusedIdProviders.Count == 0)
            {
                m_AddButton.SetEnabled(false);
            }
            else
            {
                if (unusedIdProviders.Count > 0)
                {
                    m_AddIdProviderType = new PopupField<string>(null, unusedIdProviders, 0);
                    m_AddIdProviderContainer.Add(m_AddIdProviderType);
                }

                m_AddButton.SetEnabled(true);
            }
        }

        void AddIdProvider()
        {
            var idProvider = new IdProviderResponse
            {
                New = true,
                Type = m_AddIdProviderType.value
            };

            CreateIdProviderElement(idProvider);
        }

        void OnError(Exception error)
        {
            error = AuthenticationSettingsHelper.ExtractException(error);

            m_ErrorTextElement.style.display = DisplayStyle.Flex;
            m_ErrorTextElement.text = AuthenticationSettingsHelper.ExceptionToString(error);
            Logger.LogError(error);
        }

        void CreateIdProviderElement(IdProviderResponse idProvider)
        {
            var options = IdProviderRegistry.GetOptions(idProvider.Type);
            if (options == null)
            {
                // the SDK doesn't support the ID provider type yet. Skip.
                return;
            }

            var idProviderElement = new IdProviderElement(m_IdDomainId, m_AdminClient, idProvider, options, m_SkipConfirmation);

            m_IdProviderListContainer.Add(idProviderElement);

            // Hook up events before calling Initialize.
            idProviderElement.Waiting += OnIdProviderWaiting;
            idProviderElement.Deleted += OnIdProviderDeleted;
            idProviderElement.Error += OnIdProviderError;
            idProviderElement.Initialize();

            UpdateAddIdproviderList();
        }

        void OnIdProviderWaiting(IdProviderElement sender, bool waiting)
        {
            if (waiting)
            {
                ShowWaiting();
            }
            else
            {
                HideWaiting();
            }
        }

        void OnIdProviderDeleted(IdProviderElement sender)
        {
            m_IdProviderListContainer.Remove(sender);
            UpdateAddIdproviderList();
        }

        void OnIdProviderError(IdProviderElement sender, Exception error)
        {
            OnError(error);
            HideWaiting();
        }

        void ShowWaiting()
        {
            // clear previous error when a new async action is triggered.
            m_ErrorTextElement.style.display = DisplayStyle.None;
            m_ErrorTextElement.text = string.Empty;

            m_WaitingTextElement.style.display = DisplayStyle.Flex;
            SetEnabled(false);

            Waiting?.Invoke(this, true);
        }

        void HideWaiting()
        {
            m_WaitingTextElement.style.display = DisplayStyle.None;
            SetEnabled(true);
            Waiting?.Invoke(this, false);
        }
    }
}
