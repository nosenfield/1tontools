using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OneTon.CrossSceneTrackingSystem;
using OneTon.Popups;

namespace OneTon.Popups
{
    public abstract class PopupShowEmit<TManager, E> where TManager : PopupManager<TManager, E> where E : System.Enum
    {
        protected CrossSceneTracking<TManager> manager;
        public GameObject PopupGameObject
        {
            get
            {
                return PopupScript.gameObject;
            }
        }
        public Popup PopupScript;


        public PopupShowEmit(E popupType, PopupShowOption showOption = PopupShowOption.Normal)
        {
            manager = new CrossSceneTracking<TManager>();
            PopupScript = manager.Found?.ShowNewPopup(popupType, showOption);
        }

        ~PopupShowEmit()
        {
            manager.Destroy();
        }
    }
}