using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Authentication.Editor
{
    class AdditionalAppIdElement : VisualElement
    {
        const string k_ElementUxml = "Packages/com.unity.services.authentication/Editor/UXML/AdditionalAppIdElement.uxml";
        const string k_StyleSheet = "Packages/com.unity.services.authentication/Editor/USS/AuthenticationStyleSheet.uss";

        /// <summary>
        /// The value saved on the server side.
        /// </summary>
        public AdditionalAppId SavedValue { get; }

        /// <summary>
        /// The value of that is about to be saved to the server.
        /// </summary>
        public AdditionalAppId CurrentValue { get; private set; }

        /// <summary>
        /// Event triggered when the current <cref="IdProviderElement"/> needs to be deleted by the container.
        /// The parameter of the callback is the sender.
        /// </summary>
        public event Action<AdditionalAppIdElement, bool> Deleted;

        /// <summary>
        /// Event triggered when any of the values in the additional app id change
        /// </summary>
        public event Action ChangedValues;

        /// <summary>
        /// Calculated attribute to evaluate if the additional app id has changes
        /// </summary>
        public bool Changed => SavedValue != CurrentValue;

        TextField AppIdValueTextField { get; }
        TextField AppIdDescriptionTextField { get; }
        Button DeleteButton { get; }

        readonly bool m_SkipConfirmation;

        public AdditionalAppIdElement(AdditionalAppId savedValue, bool skipConfirmation = false)
        {
            m_SkipConfirmation = skipConfirmation;

            SavedValue = savedValue;
            CurrentValue = SavedValue.Clone();

            var containerAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_ElementUxml);
            if (containerAsset != null)
            {
                var containerUI = containerAsset.CloneTree().contentContainer;
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheet);
                if (styleSheet != null)
                {
                    containerUI.styleSheets.Add(styleSheet);
                }
                else
                {
                    throw new Exception("Asset not found: " + k_StyleSheet);
                }
                AppIdValueTextField = containerUI.Q<TextField>("additional-appid-value");
                AppIdValueTextField.RegisterCallback<ChangeEvent<string>>(OnValueChanged);
                AppIdValueTextField.value = savedValue.AppId;


                AppIdDescriptionTextField = containerUI.Q<TextField>("additional-appid-description");
                AppIdDescriptionTextField.RegisterCallback<ChangeEvent<string>>(OnDescriptionChanged);
                AppIdDescriptionTextField.value = savedValue.Description;

                DeleteButton = containerUI.Q<Button>("additional-appid-delete");
                DeleteButton.clicked += OnDeleteButtonClicked;
                Add(containerUI);
            }
            else
            {
                throw new Exception("Asset not found: " + k_ElementUxml);
            }
        }

        void OnValueChanged(ChangeEvent<string> e)
        {
            CurrentValue.AppId = e.newValue;
            ChangedValues?.Invoke();
        }

        void OnDescriptionChanged(ChangeEvent<string> e)
        {
            CurrentValue.Description = e.newValue;
            ChangedValues?.Invoke();
        }

        void OnDeleteButtonClicked()
        {
            if (SavedValue.New)
            {
                InvokeDeleted(SavedValue.New);
                return;
            }
            var option = DisplayDialogComplex("Delete Request", "Do you want to delete this AppId?", "Delete", "Cancel", "");
            switch (option)
            {
                // Delete
                case 0:
                    InvokeDeleted(SavedValue.New);
                    break;

                // Cancel
                case 1:
                    break;

                default:
                    Debug.LogError("Unrecognized option.");
                    break;
            }
        }

        int DisplayDialogComplex(string title, string message, string ok, string cancel, string alt)
        {
            if (Application.isBatchMode || m_SkipConfirmation)
            {
                return 0;
            }

            return EditorUtility.DisplayDialogComplex(title, message, ok, cancel, alt);
        }

        void InvokeDeleted(bool isNew)
        {
            Deleted?.Invoke(this, isNew);
        }
    }
}
