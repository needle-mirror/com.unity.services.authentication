namespace Unity.Services.Authentication
{
    class AuthenticationSettings : IAuthenticationSettings
    {
        const int k_AccessTokenRefreshBuffer = 300;
        const int k_AccessTokenExpiryBuffer = 15;
        const int k_RefreshAttemptFrequency = 30;
        const int k_CodeConfirmationDelay = 5;

        public int AccessTokenRefreshBuffer { get; internal set; }
        public int AccessTokenExpiryBuffer { get; internal set; }
        public int RefreshAttemptFrequency { get; internal set; }
        public int CodeConfirmationAttempts { get; internal set; }
        public int CodeConfirmationDelay { get; internal set; }

        internal AuthenticationSettings()
        {
            AccessTokenRefreshBuffer = k_AccessTokenRefreshBuffer;
            AccessTokenExpiryBuffer = k_AccessTokenExpiryBuffer;
            RefreshAttemptFrequency = k_RefreshAttemptFrequency;
            CodeConfirmationDelay = k_CodeConfirmationDelay;
        }

        internal void Reset()
        {
            AccessTokenRefreshBuffer = k_AccessTokenRefreshBuffer;
            AccessTokenExpiryBuffer = k_AccessTokenExpiryBuffer;
            RefreshAttemptFrequency = k_RefreshAttemptFrequency;
            CodeConfirmationDelay = k_CodeConfirmationDelay;
        }
    }
}
