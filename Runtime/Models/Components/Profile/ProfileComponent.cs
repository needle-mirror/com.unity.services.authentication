using System;

namespace Unity.Services.Authentication
{
    class ProfileComponent : IProfile
    {
        public event Action<ProfileEventArgs> ProfileChange;

        public string Current { get => _current; set => SetProfile(value); }

        string _current;

        internal ProfileComponent(string profile)
        {
            SetProfile(profile);
        }

        public void SetProfile(string profile)
        {
            _current = profile;
            try
            {
                ProfileChange?.Invoke(new ProfileEventArgs(_current));
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }
    }
}
