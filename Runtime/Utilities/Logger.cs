using System;
using System.Diagnostics;
using Unity.Services.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Services.Authentication.Utilities
{
    static class Logger
    {
        const string k_Tag = "[Authentication]";

        const string k_VerboseLoggingDefine = "ENABLE_UNITY_AUTHENTICATION_VERBOSE_LOGGING";

        public static void Log(object message) => Debug.unityLogger.Log(k_Tag, message);
        public static void LogWarning(object message) => Debug.unityLogger.LogWarning(k_Tag, message);
        public static void LogError(object message) => Debug.unityLogger.LogError(k_Tag, message);

        public static void LogException(Exception exception)
        {
            var rex = exception as RequestFailedException;
            if (rex != null)
            {
                Debug.unityLogger.Log(LogType.Exception, k_Tag, $"[ErrorCode:{rex.ErrorCode}] {rex}");
            }
            else
            {
                Debug.unityLogger.Log(LogType.Exception, k_Tag, exception);
            }
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void LogAssertion(object message) => Debug.unityLogger.Log(LogType.Assert, k_Tag, message);

#if !ENABLE_UNITY_SERVICES_VERBOSE_LOGGING
        [Conditional(k_VerboseLoggingDefine)]
#endif
        public static void LogVerbose(object message) => Debug.unityLogger.Log(k_Tag, message);
    }
}
