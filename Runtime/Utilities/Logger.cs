using System;
using System.Diagnostics;
using Unity.Services.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Services.Authentication
{
    static class Logger
    {
        const string k_Tag = "[Authentication]";

        internal const string k_GlobalVerboseLoggingDefine = "ENABLE_UNITY_SERVICES_VERBOSE_LOGGING";
        internal const string k_AuthenticationVerboseLoggingDefine = "ENABLE_UNITY_AUTHENTICATION_VERBOSE_LOGGING";

        public static void Log(object message) => Debug.unityLogger.Log(k_Tag, message);
        public static void LogWarning(object message) => Debug.unityLogger.LogWarning(k_Tag, message);
        public static void LogError(object message) => Debug.unityLogger.LogError(k_Tag, message);
        public static void LogException(Exception exception) => Debug.unityLogger.Log(LogType.Exception, k_Tag, exception);

        [Conditional("UNITY_ASSERTIONS")]
        public static void LogAssertion(object message) => Debug.unityLogger.Log(LogType.Assert, k_Tag, message);

        [Conditional(k_GlobalVerboseLoggingDefine), Conditional(k_AuthenticationVerboseLoggingDefine)]
        public static void LogVerbose(object message) => Debug.unityLogger.Log(k_Tag, message);
    }
}
