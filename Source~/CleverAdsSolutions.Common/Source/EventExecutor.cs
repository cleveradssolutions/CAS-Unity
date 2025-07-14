//  Copyright © 2025 CAS.AI. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

namespace CAS
{
    /// <summary>
    /// Callbacks from CleverAdsSolutions are not guaranteed to be called on Unity thread.
    /// You can use EventExecutor to schedule each calls on the next Update() loop
    /// </summary>
    [WikiPage("https://github.com/cleveradssolutions/CAS-Unity/wiki/Include-Android#execute-events-on-unity-thread")]
    public static class EventExecutor
    {
        private static EventExecutorComponent instance = null;

        private static List<Action> eventsQueue = new List<Action>();
        private static List<Action> stagedEventsQueue = new List<Action>();
        private static volatile bool eventsQueueEmpty = true;
        private static int unityManagedThreadId = -1;


        /// <summary>
        /// Creation of the Executor component if needed.
        /// </summary>
        public static void Initialize()
        {
            if (instance)
                return;
            // Add an invisible game object to the scene
            GameObject obj = new GameObject("CASMainThreadExecuter");
            obj.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(obj);
            instance = obj.AddComponent<EventExecutorComponent>();
        }

        /// <summary>
        /// Is initialized already.
        /// </summary>
        public static bool IsActive()
        {
            return instance;
        }

        /// <summary>
        /// Returns true if the current thread is the Unity main thread.
        /// </summary>
        public static bool IsOnMainThread()
        {
            return Thread.CurrentThread.ManagedThreadId == unityManagedThreadId;
        }

        /// <summary>
        /// Execute action on the next Update() loop in Unity Thread.
        /// <para>Warning! To enable EventExecutor requires call once static <see cref="Initialize"/> method.</para>
        /// </summary>
        public static void Add(Action action)
        {
            if (action == null)
            {
                Debug.LogError("Event Executor skip null action");
                return;
            }
            lock (eventsQueue)
            {
                eventsQueue.Add(action);
                eventsQueueEmpty = false;
            }
        }


        public sealed class EventExecutorComponent : MonoBehaviour
        {
            private void Awake()
            {
                if (unityManagedThreadId == -1)
                {
                    unityManagedThreadId = Thread.CurrentThread.ManagedThreadId;
                }
            }

            private void Update()
            {
                if (eventsQueueEmpty)
                    return;

                lock (eventsQueue)
                {
                    stagedEventsQueue.AddRange(eventsQueue);
                    eventsQueue.Clear();
                    eventsQueueEmpty = true;
                }

                for (int i = 0; i < stagedEventsQueue.Count; i++)
                {
                    try
                    {
                        var action = stagedEventsQueue[i];
                        if (action.Target != null)
                            action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                stagedEventsQueue.Clear();
            }

            private void OnDisable()
            {
                if (instance == this)
                    instance = null;
            }
        }
    }
}