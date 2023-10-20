using System;
using System.Text;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Authentication.PlayerAccounts.Samples
{
    class PlayerAccountsDemo : MonoBehaviour
    {
        [SerializeField]
        Text m_StatusText;
        [SerializeField]
        Text m_ExceptionText;
        [SerializeField]
        GameObject m_SignOut;
        [SerializeField]
        Toggle m_PlayerAccountSignOut;

        string m_ExternalIds;

        async void Awake()
        {
            await UnityServices.InitializeAsync();
            PlayerAccountService.Instance.SignedIn +=  SignInWithUnity;
        }

        public async void StartSignInAsync()
        {
            if (PlayerAccountService.Instance.IsSignedIn)
            {
                SignInWithUnity();
                return;
            }

            try
            {
                await PlayerAccountService.Instance.StartSignInAsync();
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                SetException(ex);
            }
        }

        public void SignOut()
        {
            AuthenticationService.Instance.SignOut();

            if (m_PlayerAccountSignOut.isOn)
            {
                PlayerAccountService.Instance.SignOut();
            }

            UpdateUI();
        }

        public void OpenAccountPortal()
        {
            Application.OpenURL(PlayerAccountService.Instance.AccountPortalUrl);
        }

        async void SignInWithUnity()
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                m_ExternalIds = GetExternalIds(AuthenticationService.Instance.PlayerInfo);
                UpdateUI();
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                SetException(ex);
            }
        }

        void UpdateUI()
        {
            var statusBuilder = new StringBuilder();

            statusBuilder.AppendLine($"Player Accounts State: <b>{(PlayerAccountService.Instance.IsSignedIn ? "Signed in" : "Signed out")}</b>");
            statusBuilder.AppendLine($"Player Accounts Access token: <b>{(string.IsNullOrEmpty(PlayerAccountService.Instance.AccessToken) ? "Missing" : "Exists")}</b>\n");
            statusBuilder.AppendLine($"Authentication Service State: <b>{(AuthenticationService.Instance.IsSignedIn ? "Signed in" : "Signed out")}</b>");

            if (AuthenticationService.Instance.IsSignedIn)
            {
                m_SignOut.SetActive(true);
                statusBuilder.AppendLine(GetPlayerInfoText());
                statusBuilder.AppendLine($"PlayerId: <b>{AuthenticationService.Instance.PlayerId}</b>");
            }

            m_StatusText.text = statusBuilder.ToString();
            SetException(null);
        }

        string GetExternalIds(PlayerInfo playerInfo)
        {
            if (playerInfo.Identities == null)
            {
                return "None";
            }

            var sb = new StringBuilder();
            foreach (var id in playerInfo.Identities)
            {
                sb.Append(" " + id.TypeId);
            }

            return sb.ToString();
        }

        string GetPlayerInfoText()
        {
            return $"ExternalIds: <b>{m_ExternalIds}</b>";
        }

        void SetException(Exception ex)
        {
            m_ExceptionText.text = ex != null ? $"{ex.GetType().Name}: {ex.Message}" : "";
        }
    }
}
