using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CAS
{
    /// <summary>
    /// Callbacks from CleverAdsSolutions are not guaranteed to be called on Unity thread.
    /// You can use EventExecutor to schedule each calls on the next Update() loop
    /// </summary>
    public static class EventExecutor
    {
        private static EventExecutorComponent instance = null;

        private static List<Action> eventsQueue = new List<Action>();
        private static List<Action> startedEvents = new List<Action>();

        private static volatile bool eventsQueueEmpty = true;

        /// <summary>
        /// Creation of the Executor component if needed.
        /// </summary>
        public static void Initialize()
        {
            if (instance)
                return;
            // Add an invisible game object to the scene
            GameObject obj = new GameObject( "CASMainThreadExecuter" );
            obj.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad( obj );
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
        /// Schedule action on the next Update() loop in Unity Thread.
        /// Using EventExecutor requires call static <see cref="Initialize"/> before adding a new event.
        /// </summary>
        public static void Add( Action action )
        {
#if UNITY_EDITOR
            if (!instance)
                Debug.LogError( "Using EventExecutor requires call static Initialize() before adding a new event." );
#endif
            lock (eventsQueue)
            {
                eventsQueue.Add( action );
                eventsQueueEmpty = false;
            }
        }


        public sealed class EventExecutorComponent : MonoBehaviour
        {
            private void Update()
            {
                if (eventsQueueEmpty)
                    return;

                lock (eventsQueue)
                {
                    startedEvents.AddRange( eventsQueue );
                    eventsQueue.Clear();
                    eventsQueueEmpty = true;
                }

                for (int i = 0; i < startedEvents.Count; i++)
                {
                    var action = startedEvents[i];
                    try
                    {
                        if (action != null)
                            action.Invoke();
                        else
                            Debug.LogError( "Event Executor skip null event" );
                    }
                    catch (Exception e)
                    {
                        Debug.LogException( e );
                    }
                }
                startedEvents.Clear();
            }

            private void OnDisable()
            {
                if (instance == this)
                    instance = null;
            }
        }
    }
}