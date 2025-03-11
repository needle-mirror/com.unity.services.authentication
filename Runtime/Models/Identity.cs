namespace Unity.Services.Authentication
{
    /// <summary>
    /// Link an identity type with a user id
    /// </summary>
    public sealed class Identity
    {
        /// <summary>
        /// The identity type id.
        /// </summary>
        public string TypeId;

        /// <summary>
        /// The identity user id
        /// </summary>
        public string UserId;

        /// <summary>
        /// Constructor
        /// </summary>
        internal Identity(ExternalIdentity externalIdentity)
        {
            if (externalIdentity != null)
            {
                TypeId = externalIdentity.ProviderId;
                UserId = externalIdentity.ExternalId;
            }
        }
    }
}
