using System.Threading;
using UnityEngine;

namespace Unity.Services.Authentication
{
    static class ThreadHelper
    {
        public static bool IsMainThread => m_MainThreadId == Thread.CurrentThread.ManagedThreadId;

        static int m_MainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            m_MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }
    }
}
