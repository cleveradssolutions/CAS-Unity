using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CAS
{
    public static class EventExecutor
    {
        private static EventExecutorComponent instance = null;

        private static List<Action> eventsQueue = new List<Action>();
        private static List<Action> startedEvents = new List<Action>();

        private static volatile bool eventsQueueEmpty = true;

        public static void Initialize()
        {
            if (IsActive())
                return;
            // Add an invisible game object to the scene
            GameObject obj = new GameObject( "CASMainThreadExecuter" );
            obj.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad( obj );
            instance = obj.AddComponent<EventExecutorComponent>();
        }

        public static bool IsActive()
        {
            return instance != null;
        }

        public static void Add( Action action )
        {
            lock (eventsQueue)
            {
                eventsQueue.Add( action );
                eventsQueueEmpty = false;
            }
        }


        public sealed class EventExecutorComponent : MonoBehaviour
        {
            public void Update()
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

            public void OnDisable()
            {
                if (instance == this)
                    instance = null;
            }
        }
    }
}