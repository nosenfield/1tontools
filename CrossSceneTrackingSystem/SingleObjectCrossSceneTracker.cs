using System.Collections;
using System.Collections.Generic;
using OneTon.Singletons;
using UnityEngine;
using UnityEngine.Events;

namespace OneTon.CrossSceneTrackingSystem
{
    public class SingleObjectCrossSceneTracker : SingletonMonobehaviour<SingleObjectCrossSceneTracker>
    {
        Dictionary<System.Type, object> reported = new Dictionary<System.Type, object>();
        Dictionary<System.Type, List<UnityAction<object>>> listening = new Dictionary<System.Type, List<UnityAction<object>>>();

        public void ReportFound<T>(T toTrack) where T : class
        {
            if (toTrack != null)
            {
                if (!reported.ContainsKey(typeof(T)))
                {
                    reported.Add(typeof(T), toTrack);

                    List<UnityAction<object>> listeners;
                    if (listening.TryGetValue(typeof(T), out listeners))
                    {
                        foreach (UnityAction<object> unityAction in listeners)
                        {
                            unityAction(toTrack);
                        }
                    }
                }
            }
        }

        public void ReportLost<T>(T toTrack) where T : class
        {
            if (reported.ContainsKey(typeof(T)))
            {
                reported.Remove(typeof(T));

                List<UnityAction<object>> listeners;
                if (listening.TryGetValue(typeof(T), out listeners))
                {
                    foreach (UnityAction<object> unityAction in listeners)
                    {
                        unityAction(null);
                    }
                }
            }
        }

        public void AddListener<T>(UnityAction<object> callback) where T : class
        {
            System.Type type = typeof(T);

            if (reported.ContainsKey(type))
            {
                callback(reported[type]);
            }

            List<UnityAction<object>> listeners;
            if (!listening.TryGetValue(type, out listeners))
            {
                listeners = new List<UnityAction<object>>();
                listening.Add(type, listeners);
            }
            listeners.Add(callback);
        }

        public void RemoveListener<T>(UnityAction<object> callback)
        {
            System.Type type = typeof(T);

            if (reported.ContainsKey(type))
            {
                callback(null);
            }

            List<UnityAction<object>> listener;
            if (listening.TryGetValue(type, out listener))
            {
                listener.Remove(callback);
                if (listener.Count == 0)
                {
                    listening.Remove(type);
                }
            }
        }
    }
}