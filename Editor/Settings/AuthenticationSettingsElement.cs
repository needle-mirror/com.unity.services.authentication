using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.Editor
{
    class AuthenticationSettingsElement : VisualElement
    {
        const string k_Uxml = "Packages/com.unity.services.authentication/Editor/UXML/AuthenticationProjectSettings.uxml";
        const string k_Uss = "Packages/com.unity.services.authentication/Editor/USS/AuthenticationStyleSheet.uss";

        /// <summary>
        /// The text to show when the settings is waiting for a task to finish.
        /// </summary>
        public TextElement WaitingTextElement { get; private set; }

        /// <summary>
        /// The text to show when there is an error.
        /// </summary>
        public TextElement ErrorTextElement { get; private set; }

        /// <summary>
        /// The add ID provider choices in the dropdown list.
        /// </summary>
        public IEnumerable<string> AddIdProviderChoices { get; private set; }

        /// <summary>
        /// The add ID provider dropdown list.
        /// </summary>
        public PopupField<string> AddIdProviderName { get; private set; }

        /// <summary>
        /// The container to add a new ID provider.
        /// </summary>
        public VisualElement AddIdProviderContainer { get; private set; }

        /// <summary>
        /// The button to refresh the ID provider list.
        /// </summary>
        public Button RefreshButton { get; private set; }

        /// <summary>
        /// The button to add a new ID provider.
        /// </summary>
        public Button AddButton { get; private set; }

        /// <summary>
        /// The container to add ID providers.
        /// </summary>
        public VisualElement IdProviderListContainer { get; private set; }

        IAuthenticationAdminClient AdminClient { get; set; }
        string IdDomainId { get; set; }
        bool SkipConfirmation { get; set; }

        /// <summary>
        /// Event triggered when the <cref="AuthenticationSettingsElement"/> starts or finishes waiting for a task.
        /// The first parameter of the callback is the sender.
        /// The second parameter is true if it starts waiting, and false if it finishes waiting.
        /// </summary>
        public event Action<AuthenticationSettingsElement, bool> Waiting;

        public AuthenticationSettingsElement(IAuthenticationAdminClient adminClient, bool skipConfirmation = false)
        {
            AdminClient = adminClient;
            SkipConfirmation = skipConfirmation;

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

                WaitingTextElement = containerUI.Q<TextElement>(className: "auth-progress");
                ErrorTextElement = containerUI.Q<TextElement>(className: "auth-error");

                RefreshButton = containerUI.Q<Button>("id-provider-refresh");
                RefreshButton.clicked += RefreshButtonClicked;

                AddButton = containerUI.Q<Button>("id-provider-add");
                AddButton.visible = false;
                AddButton.clicked += AddIdProvider;

                IdProviderListContainer = containerUI.Q<VisualElement>(className: "auth-id-provider-list");
                AddIdProviderContainer = containerUI.Q<VisualElement>("id-provider-type");

                Add(containerUI);
            }
            else
            {
                throw new Exception("Asset not found: " + k_Uxml);
            }
        }

        void RefreshButtonClicked()
        {
            AuthenticationEditorAnalytics.SendRefreshEvent();
            RefreshIdProviders();
        }

        public async void RefreshIdProviders()
        {
            ShowWaiting();

            try
            {
                if (IdDomainId == null)
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
            IdDomainId = await AdminClient.GetIDDomainAsync();
        }

        async Task ListIdProvidersAsync()
        {
            IdProviderListContainer.Clear();
            var response = await AdminClient.ListIdProvidersAsync(IdDomainId);
            var results = response.Results?.ToList();

            if (results != null)
            {
                results.Sort((x, y) =>
                {
                    var optionsX = IdProviderRegistry.GetOptions(x.Type);
                    var optionsY = IdProviderRegistry.GetOptions(y.Type);
                    return optionsX?.DisplayName.CompareTo(optionsY?.DisplayName) ?? 0;
                });

                foreach (var providerResponse in results)
                {
                    CreateIdProviderElement(new IdProvider(providerResponse));
                }
            }

            UpdateAddIdProviderList();
        }

        void UpdateAddIdProviderList()
        {
            var unusedIdProviders = new List<string>(IdProviderRegistry.AllNames);

            foreach (var child in IdProviderListContainer.Children())
            {
                if (!(child is IdProviderElement))
                {
                    continue;
                }

                var idProviderElement = (IdProviderElement)child;
                if (unusedIdProviders.Contains(idProviderElement.Container.text))
                {
                    unusedIdProviders.Remove(idProviderElement.DisplayName);
                }
            }

            unusedIdProviders.Sort();

            AddIdProviderContainer.Clear();
            AddIdProviderChoices = unusedIdProviders;

            if (unusedIdProviders.Count == 0)
            {
                AddButton.visible = false;
            }
            else
            {
                if (unusedIdProviders.Count > 0)
                {
                    AddIdProviderName = new PopupField<string>(null, unusedIdProviders, 0);
                    AddIdProviderContainer.Add(AddIdProviderName);
                }

                AddButton.visible = true;
            }
        }

        void AddIdProvider()
        {
            AuthenticationEditorAnalytics.SendAddProviderEvent();

            var options = IdProviderRegistry.GetOptionsByName(AddIdProviderName.value);

            CreateIdProviderElement(new IdProvider
            {
                New = true,
                Type = options.IdProviderType
            });
        }

        void OnError(Exception error)
        {
            AuthenticationEditorAnalytics.SendErrorEvent();
            error = AuthenticationSettingsHelper.ExtractException(error);

            ErrorTextElement.style.display = DisplayStyle.Flex;
            ErrorTextElement.text = AuthenticationSettingsHelper.ExceptionToString(error);
            Logger.LogError(error);
        }

        void CreateIdProviderElement(IdProvider idProvider)
        {
            var options = IdProviderRegistry.GetOptions(idProvider.Type);
            if (options == null)
            {
                // the SDK doesn't support the ID provider type yet. Skip.
                return;
            }

            var idProviderElement = new IdProviderElement(IdDomainId, AdminClient, idProvider, options, SkipConfirmation);

            IdProviderListContainer.Add(idProviderElement);

            // Hook up events before calling Initialize.
            idProviderElement.Waiting += OnIdProviderWaiting;
            idProviderElement.Deleted += OnIdProviderDeleted;
            idProviderElement.Error += OnIdProviderError;
            idProviderElement.Initialize();

            UpdateAddIdProviderList();
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
            IdProviderListContainer.Remove(sender);
            UpdateAddIdProviderList();
        }

        void OnIdProviderError(IdProviderElement sender, Exception error)
        {
            OnError(error);
            HideWaiting();
        }

        void ShowWaiting()
        {
            // clear previous error when a new async action is triggered.
            ErrorTextElement.style.display = DisplayStyle.None;
            ErrorTextElement.text = string.Empty;

            WaitingTextElement.style.display = DisplayStyle.Flex;
            SetEnabled(false);

            Waiting?.Invoke(this, true);
        }

        void HideWaiting()
        {
            WaitingTextElement.style.display = DisplayStyle.None;
            SetEnabled(true);
            Waiting?.Invoke(this, false);
        }
    }
}
