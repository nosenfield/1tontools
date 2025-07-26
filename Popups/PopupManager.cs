using System.Collections.Generic;
using UnityEngine;
using OneTon.Dictionaries;
using OneTon.CrossSceneTrackingSystem;
using OneTon.Logging;
using OneTon.Utilities;

namespace OneTon.Popups
{
    public class PopupManager<TChild, E> : CrossSceneTrackable<TChild> where TChild : CrossSceneTrackable<TChild> where E : System.Enum
    {
        private static readonly LogService logger = LogService.Get<PopupManager<TChild, E>>();
        [SerializeField] private GameObject blocker;
        [SerializeField] private GameObject popupParent;
        [SerializeField] private GameObject popupLowerParent;
        // This is the list of all prefabs in our game.
        // They represent the core of the asset (the game object w/ script)
        // Keeping them here as a single point of failure ensures that a missing popup should be easy to identify
        [SerializeField] private EnumDictionary<E, GameObject> popupPrefabs;
        private Stack<Stack<Popup>> popupLayers;

        void Awake()
        {
            popupLayers = new Stack<Stack<Popup>>();
            popupLayers.Push(new Stack<Popup>());
        }

        void Start()
        {
            //Headache to try handling objects being in the lower but not the primary, 
            //so for now I'll just destory them. Let me know if you want me to come back 
            //and make this more robust.
            // ~ Perell
            Popup[] popupsBelow = popupLowerParent.GetComponentsInChildren<Popup>();
            for (int i = 0; i < popupsBelow.Length; i++)
            {
                Destroy(popupsBelow[i].gameObject);
            }

            blocker.SetActive(false);
            Popup[] popups = popupParent.GetComponentsInChildren<Popup>();
            for (int i = 0; i < popups.Length; i++)
            {
                popupLayers.Peek().Push(popups[i]);
                if (i < popups.Length - 1)
                {
                    popups[i].gameObject.SetActive(false);
                }
                else
                {
                    popups[i].gameObject.SetActive(true);
                    blocker.SetActive(true);
                }
            }
        }

        public Popup ShowNewPopup(E popupType, PopupShowOption showOption = PopupShowOption.Normal)
        {
            if (!popupPrefabs.Has(popupType))
            {
                logger.Error(popupType.ToString() + "not found in prefabs enum dictionary.");
                return null;
            }

            GameObject popupPrefab = popupPrefabs[popupType];
            if (popupPrefab == null)
            {
                logger.Error(popupType.ToString() + "found in prefabs enum dictionary to be empty.");
                return null;
            }

            if (popupPrefab.GetComponent<Popup>() == null)
            {
                logger.Error(popupPrefab + " has no Popup component!");
                return null;
            }

            Stack<Popup> popupStack;
            if (showOption == PopupShowOption.Additive
                && popupLayers.Peek().Count > 0)
            {
                popupLayers.Peek().Peek().transform.SetParent(popupLowerParent.transform);
                popupStack = new Stack<Popup>();
                popupLayers.Push(popupStack);
            }
            else
            {
                popupStack = popupLayers.Peek();
                if (popupStack.Count > 0)
                {
                    popupStack.Peek().gameObject.SetActive(false);
                }
            }

            blocker.SetActive(true);
            return AddPopup(popupPrefab).GetComponent<Popup>();
        }

        public string GetPopupGameObjectName(E popupType)
        {
            if (!popupPrefabs.Has(popupType))
            {
                logger.Error(popupType.ToString() + "not found in prefabs enum dictionary.");
                return null;
            }

            if (popupPrefabs[popupType] == null)
            {
                logger.Error(popupType.ToString() + "found in prefabs enum dictionary to be empty.");
                return null;
            }

            return popupPrefabs[popupType].name;
        }

        private GameObject AddPopup(GameObject popupPrefab)
        {
            GameObject popupObj = Instantiate(popupPrefab, popupParent.transform);
            popupObj.name = popupPrefab.name;
            Popup popup = popupObj.GetComponent<Popup>();
            popupLayers.Peek().Push(popup);
            popup.OnBackButton += RespondToBackButton;
            popup.OnCloseButton += RespondToCloseButton;
            return popupObj;
        }

        private void RespondToCloseButton()
        {
            Stack<Popup> popupStack = popupLayers.Peek();
            foreach (Popup popup in popupStack)
            {
                DestroyPopup(popup);
            }
            popupStack.Clear();

            if (popupLayers.Count == 1)
            {
                blocker.SetActive(false);
            }
            else
            {
                popupLayers.Pop();
                popupStack = popupLayers.Peek();
                popupStack.Peek().transform.SetParent(popupParent.transform);
            }
        }

        private void RespondToBackButton()
        {
            Stack<Popup> popupStack = popupLayers.Peek();
            DestroyPopup(popupStack.Pop());

            if (popupStack.Count > 0)
            {
                popupStack.Peek().gameObject.SetActive(true);
            }
            else
            {
                if (popupLayers.Count == 1)
                {
                    blocker.SetActive(false);
                }
                else
                {
                    popupLayers.Pop();
                    popupStack = popupLayers.Peek();
                    popupStack.Peek().transform.SetParent(popupParent.transform);
                }
            }
        }

        private void DestroyPopup(Popup popup)
        {
            popup.OnBackButton -= RespondToBackButton;
            popup.OnCloseButton -= RespondToCloseButton;
            GameObject.Destroy(popup.gameObject);
        }

    }
}