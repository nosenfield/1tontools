using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace OneTon.CrossSceneTrackingSystem
{
    public class CrossSceneTracking<T> where T : class, ICrossSceneTrackable
    {
        T found = null;

        private UnityAction foundCallback, lostCallback;

        public CrossSceneTracking(UnityAction _foundCallback = null, UnityAction _lostCallback = null)
        {
            foundCallback = _foundCallback;
            lostCallback = _lostCallback;
            SingleObjectCrossSceneTracker.Instance.AddListener<T>(SetFound);
        }

        public void Destroy()
        {
            SingleObjectCrossSceneTracker.Instance.RemoveListener<T>(SetFound);
        }

        ~CrossSceneTracking()
        {
            Destroy();
        }

        public T Found
        {
            get
            {
                return found;
            }
        }


        private void SetFound(object obj)
        {
            if (found != null && lostCallback != null)
            {
                lostCallback();
            }
            found = (T)obj;
            if (found != null && foundCallback != null)
            {
                OnFound();
            }
        }

        private async void OnFound()
        {
            await Task.Delay(1); //Without the delay, Unity struggles to load the full object in. So although it will be correct that found does not equal null, it will lead to errors being thrown when the callback functions try to access found.
            foundCallback();
        }
    }
}
