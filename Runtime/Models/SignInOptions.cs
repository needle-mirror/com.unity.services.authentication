namespace Unity.Services.Authentication
{
    /// <summary>
    /// Options for sign operations
    /// </summary>
    public sealed class SignInOptions
    {
        /// <summary>
        /// Option to create an account if none exist.
        /// </summary>
        public bool CreateAccount { get; set; }
    }
}
