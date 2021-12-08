using System;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains the current profile
    /// </summary>
    interface IProfile
    {
        /// <summary>
        /// Event triggered when the current profile changes.
        /// </summary>
        event Action<ProfileEventArgs> ProfileChange;

        /// <summary>
        /// Returns the profile if one is set
        /// </summary>
        string Current { get; set; }
    }
}
