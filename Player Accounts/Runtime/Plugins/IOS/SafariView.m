#import <AuthenticationServices/AuthenticationServices.h>

// The Swift Xcode project type removes UnityGetGLViewController(); use the UnityPlayer singleton instead.
#if UNITY_XCODE_PROJECT_TYPE_SWIFT
#import <UnityFramework/UnityFramework-Swift.h>
#else
extern UIViewController* UnityGetGLViewController();
#endif

// Callback signature used to deliver the OAuth redirect (or an error) back to C#.
// callbackUrl: the full custom-scheme URL the OAuth server redirected to (e.g. unitydl://...?code=...&state=...)
// error:       a non-empty, human readable string when the session failed or was cancelled by the user.
typedef void (*UnityPlayerAccountAuthCallback)(const char *callbackUrl, const char *error);

API_AVAILABLE(ios(13.0))
@interface UnityPlayerAccountPresentationContext : NSObject<ASWebAuthenticationPresentationContextProviding>
@end
@implementation UnityPlayerAccountPresentationContext
- (nonnull ASPresentationAnchor)presentationAnchorForWebAuthenticationSession:(nonnull ASWebAuthenticationSession *)session API_AVAILABLE(ios(13.0))
{
#if UNITY_XCODE_PROJECT_TYPE_SWIFT
    return [[UnityPlayer shared] rootViewController].view.window;
#else
    return UnityGetGLViewController().view.window;
#endif
}
@end

static UnityPlayerAccountAuthCallback s_AuthCallback = NULL;
static ASWebAuthenticationSession *s_Session = nil;
API_AVAILABLE(ios(13.0))
static UnityPlayerAccountPresentationContext *s_PresentationContext = nil;

// C-linkage entry points invoked from C# via DllImport("__Internal"). This file must stay .m
// (not .mm) so the Swift project type's UnityFramework-Swift.h, which uses @import, compiles.

// Registers the C# function pointer that the auth session completion handler invokes.
void setUnityPlayerAccountAuthCallback(UnityPlayerAccountAuthCallback callback)
{
    s_AuthCallback = callback;
}

void launchUnityPlayerAccountUrl(const char *url, const char *callbackScheme)
{
    NSURL *authURL = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
    NSString *scheme = callbackScheme != NULL ? [NSString stringWithUTF8String:callbackScheme] : nil;

    // ASWebAuthenticationSession is purpose-built for OAuth: it intercepts the
    // redirect to `callbackScheme://...`, hands the full URL back here, and
    // dismisses its own UI automatically.
    s_Session = [[ASWebAuthenticationSession alloc]
        initWithURL:authURL
        callbackURLScheme:scheme
        completionHandler:^(NSURL *callbackURL, NSError *error) {
            if (s_AuthCallback != NULL)
            {
                // NOTE: it may look odd that we are holding onto these static variables only to null them,
                // but this is required to prevent automatic deallocation. If you do not keep these references
                // for the duration, the session will return immediately with an unhelpful error message.
                // See: https://developer.apple.com/documentation/authenticationservices/authenticating-a-user-through-a-web-service
                s_Session = nil;
                if (@available(iOS 13.0, *))
                {
                    s_PresentationContext = nil;
                }

                const char *callbackString = callbackURL != nil ? [callbackURL.absoluteString UTF8String] : "";
                const char *errorString = error != nil ? [error.localizedDescription UTF8String] : "";
                s_AuthCallback(callbackString, errorString);
            }
        }];

    // The minimum supported version of Unity for the Auth SDK is currently 2022.3, which allows you to make
    // builds targeting iOS 12. Therefore we do need this iOS 13+ check for setting the presentationContextProvider.
    // If the SDK's minimum support Unity version ever goes up, we may be able to remove this guard.
    if (@available(iOS 13.0, *))
    {
        s_PresentationContext = [[UnityPlayerAccountPresentationContext alloc] init];
        s_Session.presentationContextProvider = s_PresentationContext;
    }

    [s_Session start];
}