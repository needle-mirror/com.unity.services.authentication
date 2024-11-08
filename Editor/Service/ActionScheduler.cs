using System;
using System.Collections.Generic;
using Unity.Services.Core.Scheduler.Internal;
using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    /// <inheritdoc />
    class ActionScheduler : IActionScheduler
    {
        internal class ScheduledInvocation
        {
            public Action Action;
            public DateTime InvocationTime;
            public long ActionId;
        }

        internal readonly Dictionary<long, ScheduledInvocation> Actions = new Dictionary<long, ScheduledInvocation>();
        internal readonly List<ScheduledInvocation> ReadyActions = new List<ScheduledInvocation>();

        const long k_MinimumIdValue = 1;
        readonly object m_Lock = new object();
        long m_NextId = k_MinimumIdValue;

        public ActionScheduler()
        {
            EditorApplication.update += Update;
        }

        public long ScheduleAction(Action action, double delaySeconds = 0)
        {
            lock (m_Lock)
            {
                if (delaySeconds < 0)
                {
                    throw new ArgumentException("delaySeconds can not be negative");
                }

                if (action is null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                var actionId = m_NextId++;

                Actions.Add(actionId, new ScheduledInvocation
                {
                    Action = action,
                    InvocationTime = DateTime.Now.AddSeconds(delaySeconds),
                    ActionId = actionId
                });

                if (m_NextId < k_MinimumIdValue)
                {
                    m_NextId = k_MinimumIdValue;
                }

                return actionId;
            }
        }

        public void CancelAction(long actionId)
        {
            lock (m_Lock)
            {
                Actions.Remove(actionId);
            }
        }

        void Update()
        {
            lock (m_Lock)
            {
                ReadyActions.Clear();

                foreach (var action in Actions)
                {
                    if (action.Value.InvocationTime <= DateTime.Now)
                    {
                        ReadyActions.Add(action.Value);
                    }
                }

                foreach (var readyAction in ReadyActions)
                {
                    try
                    {
                        Actions.Remove(readyAction.ActionId);
                        readyAction.Action();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
