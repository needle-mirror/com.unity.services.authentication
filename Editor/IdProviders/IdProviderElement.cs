using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.Editor
{
    class IdProviderElement : VisualElement
    {
        const string k_ElementUxml = "Packages/com.unity.services.authentication/Editor/UXML/IdProviderElement.uxml";
        const string k_IdProviderNameRegex = @"^oidc-[a-z0-9-_\.]{1,15}$";
        const string k_NewOpenIDProviderName = "New OpenId Connect";

        /// <summary>
        /// Event triggered when the <see cref="IdProviderElement"/> starts or finishes waiting for a task.
        /// The first parameter of the callback is the sender.
        /// The second parameter is true if it starts waiting, and false if it finishes waiting.
        /// </summary>
        public event Action<IdProviderElement, bool> Waiting;

        /// <summary>
        /// Event triggered when the current <see cref="IdProviderElement"/> needs to be deleted by the container.
        /// The parameter of the callback is the sender.
        /// </summary>
        public event Action<IdProviderElement> Deleted;

        /// <summary>
        /// Event triggered when the current <see cref="IdProviderElement"/> catches an error.
        /// The first parameter of the callback is the sender.
        /// The second parameter is the exception caught by the element.
        /// </summary>
        public event Action<IdProviderElement, Exception> Error;

        /// <summary>
        /// The foldout container to show or hide the ID provider details.
        /// </summary>
        public Foldout Container { get; }

        /// <summary>
        /// The display name to show.
        /// </summary>
        public string DisplayName => m_Options.DisplayName;

        /// <summary>
        /// The provider type id.
        /// </summary>
        public string Type => m_Options.IdProviderType;

        /// <summary>
        /// The toggle to control whether the ID provider is enabled.
        /// </summary>
        public Toggle EnabledToggle { get; }

        /// <summary>
        /// The text field to fill the client ID.
        /// </summary>
        public TextField ClientIdField { get; }

        /// <summary>
        /// The text field to fill the client secret.
        /// </summary>
        public TextField ClientSecretField { get; }

        /// <summary>
        /// The text field to fill the oidc name.
        /// </summary>
        public TextField OidcName { get; }

        /// <summary>
        /// The text field to fill the oidc issuer url.
        /// </summary>
        public TextField OidcIssuer { get; }

        /// <summary>
        /// The button to save the changes.
        /// </summary>
        public Button SaveButton { get; }

        /// <summary>
        /// The button to cancel changes.
        /// </summary>
        public Button CancelButton { get; }

        /// <summary>
        /// The button to delete the current ID provider.
        /// </summary>
        public Button DeleteButton { get; }

        /// <summary>
        /// The optional additional custom settings element for the ID provider.
        /// </summary>
        public IdProviderCustomSettingsElement CustomSettingsElement { get; }

        /// <summary>
        /// The value saved on the server side.
        /// </summary>
        public IdProvider SavedValue { get; set; }

        /// <summary>
        /// The value of that is about to be saved to the server.
        /// </summary>
        public IdProvider CurrentValue { get; set; }

        public bool Changed =>
            SavedValue?.Type != CurrentValue?.Type ||
            SavedValue?.Disabled != CurrentValue?.Disabled ||
            (SavedValue?.ClientId ?? "") != (CurrentValue?.ClientId ?? "") ||
            (SavedValue?.ClientSecret ?? "") != (CurrentValue?.ClientSecret ?? "") ||
            (SavedValue?.OidcConfig.Issuer ?? "") != (CurrentValue?.OidcConfig.Issuer ?? "");

        public bool IsValid =>
            !string.IsNullOrEmpty(CurrentValue.ClientId) &&
            (!m_Options.NeedClientSecret || !string.IsNullOrEmpty(CurrentValue.ClientSecret)) &&
            (m_Options.OidcConfig.Issuer == null || !string.IsNullOrEmpty(CurrentValue.OidcConfig.Issuer));

        readonly string m_IdDomainId;
        readonly IAuthenticationAdminClient m_AdminClient;
        readonly IdProviderOptions m_Options;
        // Whether skip the confirmation window for tests/automation.
        readonly bool m_SkipConfirmation;

        /// <summary>
        /// Configures the Id provider element
        /// </summary>
        /// <param name="idDomain"> The ID domain associated with the project </param>
        /// <param name="adminClient"> API client to the Admin APIs of the Authentication service </param>
        /// <param name="savedValue"> The value saved on the server side </param>
        /// <param name="options"> The ID provider metadata that is used to render the settings UI </param>
        /// <param name="skipConfirmation"> Whether to skip the confirmation window for tests/automation </param>
        public IdProviderElement(string idDomain, IAuthenticationAdminClient adminClient, IdProvider savedValue, IdProviderOptions options, bool skipConfirmation = false)
        {
            m_IdDomainId = idDomain;
            m_AdminClient = adminClient;
            m_Options = options;
            m_SkipConfirmation = skipConfirmation;

            SavedValue = savedValue;
            CurrentValue = SavedValue.Clone();

            var containerAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_ElementUxml);
            if (containerAsset != null)
            {
                var containerUI = containerAsset.CloneTree().contentContainer;

                Container = containerUI.Q<Foldout>(className: "auth-id-provider-details");
                Container.text = m_Options.IdProviderType.Contains(IdProviderKeys.OpenIDConnect) ? savedValue.Type : m_Options.DisplayName;
                Container.text = Container.text == IdProviderKeys.OpenIDConnect ? k_NewOpenIDProviderName : Container.text;

                // If the ID Provider element is new, default to unfold.
                Container.value = SavedValue.New;

                EnabledToggle = containerUI.Q<Toggle>("id-provider-enabled");
                EnabledToggle.RegisterCallback<ChangeEvent<bool>>(OnEnabledChanged);

                ClientIdField = containerUI.Q<TextField>("id-provider-client-id");
                ClientIdField.label = options.ClientIdDisplayName;
                ClientIdField.RegisterCallback<ChangeEvent<string>>(OnClientIdChanged);

                ClientSecretField = containerUI.Q<TextField>("id-provider-client-secret");
                if (options.NeedClientSecret)
                {
                    ClientSecretField.label = options.ClientSecretDisplayName;
                    ClientSecretField.RegisterCallback<ChangeEvent<string>>(OnClientSecretChanged);
                }
                else
                {
                    ClientSecretField.style.display = DisplayStyle.None;
                }

                OidcIssuer = containerUI.Q<TextField>("oidc-issuer-url");
                OidcName = containerUI.Q<TextField>("oidc-name");

                if (options.OidcConfig.Issuer != null)
                {
                    OidcIssuer.RegisterCallback<ChangeEvent<string>>(OnIssuerURLChanged);
                    OidcName.isReadOnly = Container.text == savedValue.Type;
                    OidcName.RegisterCallback<ChangeEvent<string>>(OnOidcNameChanged);
                }
                else
                {
                    OidcIssuer.style.display = DisplayStyle.None;
                    OidcName.style.display = DisplayStyle.None;
                }

                SaveButton = containerUI.Q<Button>("id-provider-save");
                SaveButton.SetEnabled(false);
                SaveButton.clicked += OnSaveButtonClicked;

                CancelButton = containerUI.Q<Button>("id-provider-cancel");
                CancelButton.SetEnabled(false);
                CancelButton.clicked += OnCancelButtonClicked;

                DeleteButton = containerUI.Q<Button>("id-provider-delete");
                DeleteButton.clicked += OnDeleteButtonClicked;
                DeleteButton.SetEnabled(!savedValue.New);

                if (options.CustomSettingsElementCreator != null)
                {
                    CustomSettingsElement = options.CustomSettingsElementCreator.Invoke(m_IdDomainId, () => m_AdminClient.GatewayToken, skipConfirmation);
                    Container.Add(CustomSettingsElement);
                    CustomSettingsElement.Waiting += OnAdditionalElementWaiting;
                    CustomSettingsElement.Error += OnAdditionalElementError;
                }

                ResetCurrentValue();
                Add(containerUI);
            }
            else
            {
                throw new Exception("Asset not found: " + k_ElementUxml);
            }
        }

        /// <summary>
        /// Initialize the ID provider element in case any server side API call is needed.
        /// Now the ID provider itself doesn't need additional API calls. It's up to the additional settings.
        /// </summary>
        public void Initialize()
        {
            if (!SavedValue.New)
            {
                CustomSettingsElement?.Refresh();
            }
        }

        void RefreshButtons()
        {
            var hasChanges = Changed;
            SaveButton.SetEnabled(hasChanges && IsValid);
            CancelButton.SetEnabled(hasChanges || SavedValue.New);
            if (SavedValue.New)
            {
                DeleteButton.SetEnabled(false);
                DeleteButton.style.display = DisplayStyle.None;
                CustomSettingsElement?.SetEnabled(false);
            }
            else
            {
                DeleteButton.SetEnabled(true);
                DeleteButton.style.display = DisplayStyle.Flex;
                CustomSettingsElement?.SetEnabled(true);
            }
        }

        void OnEnabledChanged(ChangeEvent<bool> e)
        {
            CurrentValue.Disabled = !e.newValue;
            RefreshButtons();
        }

        void OnClientIdChanged(ChangeEvent<string> e)
        {
            CurrentValue.ClientId = e.newValue;
            RefreshButtons();
        }

        void OnClientSecretChanged(ChangeEvent<string> e)
        {
            CurrentValue.ClientSecret = e.newValue;
            RefreshButtons();
        }

        void OnOidcNameChanged(ChangeEvent<string> evt)
        {
            CurrentValue.Type = evt.newValue;
            RefreshButtons();
        }

        void OnIssuerURLChanged(ChangeEvent<string> evt)
        {
            var currentValueOidcConfig = CurrentValue.OidcConfig;
            currentValueOidcConfig.Issuer = evt.newValue;
            CurrentValue.OidcConfig = currentValueOidcConfig;
            RefreshButtons();
        }

        void OnSaveButtonClicked()
        {
            if (!ValidateOpenIDConnectParams())
            {
                return;
            }

            var option = DisplayDialogComplex("Save your changes", "Do you want to save the ID provider changes?", "Save", "Cancel", "");
            switch (option)
            {
                case 0:
                    AuthenticationEditorAnalytics.SendSaveProviderEvent();
                    Save();
                    break;

                case 1:
                    break;

                default:
                    Logger.LogError("Unrecognized option.");
                    break;
            }
        }

        bool ValidateOpenIDConnectParams()
        {
            try
            {
                if (CurrentValue.OidcConfig.Issuer != null)
                {
                    if (!ValidateOpenIdConnectIdProviderName(CurrentValue.Type))
                    {
                        throw new ArgumentException("Invalid Oidc Name: The Id Provider name should start with 'oidc-' and have between 1 and 20 characters");
                    }

                    if (!Uri.IsWellFormedUriString(CurrentValue.OidcConfig.Issuer, UriKind.Absolute))
                    {
                        throw new ArgumentException("Invalid Issuer(URL): The URL should begin with 'http://' or 'https://' and include your domain name");
                    }

                    Container.text = CurrentValue.Type;
                    OidcName.isReadOnly = true;
                }
            }
            catch (Exception e)
            {
                InvokeError(e);
                return false;
            }

            return true;
        }

        static bool ValidateOpenIdConnectIdProviderName(string idProviderName)
        {
            return !string.IsNullOrEmpty(idProviderName) && Regex.Match(idProviderName, k_IdProviderNameRegex).Success;
        }

        async void Save()
        {
            InvokeWaiting(true);

            try
            {
                if (SavedValue.New)
                {
                    var request = new CreateIdProviderRequest(CurrentValue);
                    var response = await m_AdminClient.CreateIdProviderAsync(m_IdDomainId, request);
                    SavedValue = new IdProvider(response);
                }
                else
                {
                    var request = new UpdateIdProviderRequest(CurrentValue);
                    var response = await m_AdminClient.UpdateIdProviderAsync(m_IdDomainId, CurrentValue.Type, request);
                    SavedValue = new IdProvider(response);
                }


                await UpdateStateAsync();
                OnSaveCompleted();
            }
            catch (Exception e)
            {
                InvokeError(e);
            }

            InvokeWaiting(false);
        }

        async Task UpdateStateAsync()
        {
            if (SavedValue.Disabled != CurrentValue.Disabled)
            {
                SavedValue.ClientSecret = CurrentValue.ClientSecret;
                var task = CurrentValue.Disabled ? m_AdminClient.DisableIdProviderAsync(m_IdDomainId, CurrentValue.Type) : m_AdminClient.EnableIdProviderAsync(m_IdDomainId, CurrentValue.Type);
                var response = await task;

                // Only reset current value when no exception
                SavedValue = new IdProvider(response);
                ResetCurrentValue();
                RefreshButtons();
            }
        }

        void OnSaveCompleted()
        {
            // Enable/disable is not changed
            ResetCurrentValue();
            RefreshButtons();

            // Refresh the additional settings if necessary.
            CustomSettingsElement?.Refresh();
        }

        void OnCancelButtonClicked()
        {
            if (SavedValue.New)
            {
                // It's a new ID provider and it hasn't been saved to the server yet.
                // Simply trigger delete event to notify parent to remove the element from the list.
                Deleted?.Invoke(this);
            }
            else
            {
                ResetCurrentValue();
            }
        }

        void ResetCurrentValue()
        {
            CurrentValue = SavedValue.Clone();
            EnabledToggle.value = !CurrentValue.Disabled;
            ClientIdField.value = CurrentValue.ClientId ?? "";
            ClientSecretField.value = CurrentValue.ClientSecret ?? "";
            OidcName.value = CurrentValue.Type ?? "";
            OidcIssuer.value = CurrentValue.OidcConfig.Issuer ?? "";
            RefreshButtons();
        }

        void OnDeleteButtonClicked()
        {
            var option = DisplayDialogComplex("Delete Request", "Do you want to delete the ID Provider?", "Delete", "Cancel", "");
            switch (option)
            {
                // Delete
                case 0:
                    Delete();
                    break;

                // Cancel
                case 1:
                    break;

                default:
                    Logger.LogError("Unrecognized option.");
                    break;
            }
        }

        async void Delete()
        {
            InvokeWaiting(true);

            try
            {
                await m_AdminClient.DeleteIdProviderAsync(m_IdDomainId, CurrentValue.Type);
                Deleted?.Invoke(this);
                ResetCurrentValue();
            }
            catch (Exception e)
            {
                InvokeError(e);
            }

            InvokeWaiting(false);
        }

        int DisplayDialogComplex(string title, string message, string ok, string cancel, string alt)
        {
            if (Application.isBatchMode || m_SkipConfirmation)
            {
                return 0;
            }

            return EditorUtility.DisplayDialogComplex(title, message, ok, cancel, alt);
        }

        void OnAdditionalElementWaiting(IdProviderCustomSettingsElement sender, bool waiting)
        {
            InvokeWaiting(waiting);
        }

        void OnAdditionalElementError(IdProviderCustomSettingsElement sender, Exception error)
        {
            InvokeError(error);
        }

        void InvokeWaiting(bool waiting)
        {
            Waiting?.Invoke(this, waiting);
        }

        void InvokeError(Exception ex)
        {
            Error?.Invoke(this, AuthenticationSettingsHelper.ExtractException(ex));
        }
    }
}
