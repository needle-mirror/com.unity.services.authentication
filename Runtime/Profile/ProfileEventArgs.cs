using System;

namespace Unity.Services.Authentication
{
    class ProfileEventArgs : EventArgs
    {
        public string Profile { get; }

        public ProfileEventArgs(string profile)
        {
            Profile = profile;
        }
    }
}
