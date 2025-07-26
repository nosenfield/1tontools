using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using OneTon.Logging;

namespace OneTon.Utilities
{
    public class SingletonPrefabMarker : MonoBehaviour
    {
        private static readonly LogService logger = LogService.Get<SingletonPrefabMarker>();
        [ShowInInspector] private List<SingletonPrefabMarker> deactivatedPrefabs;
        [SerializeField] private SingletonPrefab prefab;
        [SerializeField] private GameObject rootGameObject;

        void OnEnable()
        {
            if (prefab.Instance != null && prefab.Instance != this)
            {
                logger.Debug($"{prefab.name} already has instance {prefab.Instance.name}. Deactivating {name}");
                if (!prefab.Instance.deactivatedPrefabs.Contains(this))
                {
                    prefab.Instance.deactivatedPrefabs.Add(this);
                }
                rootGameObject.SetActive(false);
            }
            else
            {
                logger.Debug($"{prefab.name} set to {name}");
                prefab.Instance = this;
                deactivatedPrefabs = new();
            }
        }

        void OnDestroy()
        {
            logger.Trace();

            if (prefab.Instance == this)
            {
                if (deactivatedPrefabs.Count > 0)
                {
                    logger.Debug($"Activating next instance of SingletonPrefab:{deactivatedPrefabs[0].rootGameObject.name}");
                    prefab.Instance = deactivatedPrefabs[0];
                    deactivatedPrefabs.Remove(prefab.Instance);
                    prefab.Instance.rootGameObject.SetActive(true);
                    prefab.Instance.deactivatedPrefabs = deactivatedPrefabs;
                }
                else
                {
                    prefab.Instance = null;
                }
            }
            else
            {
                prefab.Instance.deactivatedPrefabs.Remove(this);
            }
        }
    }
}