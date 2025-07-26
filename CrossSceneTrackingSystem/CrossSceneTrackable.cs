using OneTon.Singletons;
using UnityEngine;

namespace OneTon.CrossSceneTrackingSystem
{
    public interface ICrossSceneTrackable
    {
    }

    public abstract class CrossSceneTrackable<T> : MonoBehaviour, ICrossSceneTrackable where T : CrossSceneTrackable<T>
    {
        protected virtual void OnEnable()
        {
            SingleObjectCrossSceneTracker.Instance.ReportFound<T>((T)this);
        }

        protected virtual void OnDisable()
        {
            SingleObjectCrossSceneTracker.Instance.ReportLost<T>((T)this);
        }
    }

    public abstract class CrossSceneTrackableSingleton<T> : SingletonMonobehaviour<T>, ICrossSceneTrackable where T : CrossSceneTrackableSingleton<T>
    {
        protected virtual void OnEnable()
        {
            SingleObjectCrossSceneTracker.Instance.ReportFound<T>((T)this);
        }

        protected virtual void OnDisable()
        {
            SingleObjectCrossSceneTracker.Instance.ReportLost<T>((T)this);
        }
    }
}