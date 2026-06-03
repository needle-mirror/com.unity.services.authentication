
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Manages targeting state, attributes, and release context for the current player session.
    /// </summary>
    /// <remarks>
    /// Targeting is automatically enabled when any package registers as a consumer via
    /// <see cref="ITargetingActivation.RegisterConsumer"/>. This default behavior can be overridden
    /// by calling <see cref="SetEnabled"/> and restored by calling <see cref="ClearEnabled"/>.
    /// </remarks>
    public class TargetingComponent : ITargetingContext, ITargetingActivation
    {
        bool? _developerOverride;
        readonly HashSet<string> _consumers = new HashSet<string>();
        bool _warnedAboutDisableWhileConsumers;

        /// <summary>
        /// Whether targeting is currently enabled.
        /// </summary>
        /// <remarks>
        /// Returns the value set by <see cref="SetEnabled"/> if called, otherwise returns
        /// <c>true</c> when one or more packages have registered as targeting consumers.
        /// </remarks>
        public bool Enabled => _developerOverride ?? _consumers.Count > 0;

        readonly Attributes _attributes;

        /// <summary>
        /// The targeting attributes that will be sent with the next sign-in or refresh request.
        /// </summary>
        public Attributes Attributes => _attributes;

        /// <summary>
        /// The variant tags assigned to this session by the server, or <c>null</c> if no release has been resolved.
        /// </summary>
        public string[] VariantTags { get; internal set; }

        internal AuthenticationServiceInternal AuthenticationService;

        internal TargetingComponent()
        {
            _attributes = new Attributes();
        }

        /// <summary>
        /// Explicitly enables or disables targeting, overriding the default behavior.
        /// </summary>
        /// <remarks>
        /// By default, targeting is enabled automatically when any package registers as a consumer.
        /// Calling this method overrides that behavior. If set to <c>false</c> while registered
        /// consumers exist, a warning is logged. Call <see cref="ClearEnabled"/> to restore the
        /// default consumer-based behavior.
        /// </remarks>
        /// <param name="enabled">Set to <c>true</c> to enable targeting, or <c>false</c> to disable it.</param>
        public void SetEnabled(bool enabled)
        {
            _developerOverride = enabled;

            if (enabled)
            {
                _warnedAboutDisableWhileConsumers = false;
                return;
            }

            if (_consumers.Count > 0 && !_warnedAboutDisableWhileConsumers)
            {
                var names = string.Join(", ", _consumers.OrderBy(n => n));
                Logger.LogWarning(
                    $"Targeting was disabled by the application, but the following packages depend on it " +
                    $"and may not function correctly: [{names}]. Call " +
                    $"AuthenticationService.Instance.Targeting.ClearEnabled() to restore.");
                _warnedAboutDisableWhileConsumers = true;
            }
        }

        /// <summary>
        /// Clears any value set by <see cref="SetEnabled"/>, restoring the default behavior where
        /// targeting is enabled automatically when registered consumers exist.
        /// </summary>
        public void ClearEnabled()
        {
            _developerOverride = null;
            _warnedAboutDisableWhileConsumers = false;
        }

        void ITargetingActivation.RegisterConsumer(string consumerName)
        {
            if (string.IsNullOrEmpty(consumerName))
            {
                return;
            }
            _consumers.Add(consumerName);
        }

        /// <summary>
        /// Sets custom user-scoped attributes to be included in targeting requests.
        /// </summary>
        /// <param name="userAttributes">An object whose properties will be serialized as user attributes.</param>
        public void SetUserAttributes(object userAttributes)
        {
            _attributes.User = userAttributes;
        }

        /// <summary>
        /// Sets custom application-scoped attributes to be included in targeting requests.
        /// </summary>
        /// <param name="appAttributes">An object whose properties will be serialized as application attributes.</param>
        public void SetAppAttributes(object appAttributes)
        {
            _attributes.App = appAttributes;
        }

        /// <summary>
        /// Clears any custom user attributes previously set by <see cref="SetUserAttributes"/>.
        /// </summary>
        public void ClearUserAttributes()
        {
            _attributes.User = null;
        }

        /// <summary>
        /// Clears any custom application attributes previously set by <see cref="SetAppAttributes"/>.
        /// </summary>
        public void ClearAppAttributes()
        {
            _attributes.App = null;
        }

        /// <summary>
        /// Refreshes the current session using the stored session token, sending targeting attributes if targeting is enabled.
        /// </summary>
        /// <returns>A task for the refresh operation.</returns>
        /// <exception cref="AuthenticationException">
        /// The task fails with the exception when the session token does not exist.
        /// </exception>
        public Task RefreshAsync()
        {
            if (AuthenticationService.SessionTokenExists)
            {
                var sessionToken = AuthenticationService.SessionTokenComponent.SessionToken;

                if (string.IsNullOrEmpty(sessionToken))
                {
                    AuthenticationService.SessionTokenComponent.Clear();
                    var exception = AuthenticationService.ExceptionHandler.BuildClientSessionTokenNotExistsException();
                    AuthenticationService.SendSignInFailedEvent(exception, true);
                    return Task.FromException(exception);
                }

                AuthenticationService.ChangeState(AuthenticationState.Refreshing);
                return AuthenticationService.HandleSignInRequestAsync(() => AuthenticationService.NetworkClient.SignInWithSessionTokenAsync(new SessionTokenRequest
                {
                    SessionToken = sessionToken,
                    Attributes = Enabled ? Attributes : null
                }));
            }
            else
            {
                AuthenticationService.SessionTokenComponent.Clear();
                var exception = AuthenticationService.ExceptionHandler.BuildClientSessionTokenNotExistsException();
                AuthenticationService.SendSignInFailedEvent(exception, true);
                return Task.FromException(exception);
            }
        }
    }
}
