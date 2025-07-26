using System;
using UnityEngine;
using UnityEngine.Events;

namespace OneTon.Popups
{
    public abstract class Popup : MonoBehaviour
    {
        public event UnityAction OnCloseButton;
        public event UnityAction OnBackButton;

        // Setting the callback to instantiate a new popup could cause that popup to be closed by PopupManager's OnCloseButton listener that is invoked after the callback
        // If, upon closing a popup, we wish to give rise to another popup outside the existing "back" or "close" functionality,
        // we can map the "OnClick" function to a public function of the popup content script
        // That function can call a new popup emit that will effectively close it.
        public Action Callback;

        public virtual void CloseButtonOnClick()
        {
            Callback?.Invoke();
            OnCloseButton?.Invoke();
        }

        public virtual void BackButtonOnClick()
        {
            OnBackButton?.Invoke();
        }
    }
}