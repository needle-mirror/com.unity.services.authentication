using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class WellKnownKeysComponent
    {
        public WellKnownKey[] Keys { get; internal set; }

        internal WellKnownKeysComponent()
        {
        }
    }
}
