using System;

namespace Unity.Services.Authentication
{
    class Profile : IProfile
    {
        public event Action<ProfileEventArgs> ProfileChange;

        public string Current { get => _current; set => SetProfile(value); }

        string _current;

        internal Profile(string profile)
        {
            SetProfile(profile);
        }

        public void SetProfile(string profile)
        {
            _current = profile;
            ProfileChange?.Invoke(new ProfileEventArgs(_current));
        }
    }
}
