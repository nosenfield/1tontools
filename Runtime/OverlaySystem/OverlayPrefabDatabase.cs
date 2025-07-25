using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneTon.OverlaySystem
{
    [CreateAssetMenu(menuName = "1ton/OverlaySystem/OverlayPrefabDatabase")]
    public class OverlayPrefabDatabase : ScriptableObject
    {
        [SerializeField]
        private List<GameObject> prefabs;

        private Dictionary<Type, GameObject> _typeToPrefab;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _typeToPrefab = new Dictionary<Type, GameObject>();

            foreach (var prefab in prefabs)
            {
                if (prefab == null) continue;

                var component = prefab.GetComponent<OverlayBase>();
                if (component == null)
                {
                    Debug.LogWarning($"{prefab.name} does not contain a OverlayBase-derived component.");
                    continue;
                }

                var type = component.GetType();
                if (!_typeToPrefab.ContainsKey(type))
                {
                    _typeToPrefab[type] = prefab;
                }
                else
                {
                    Debug.LogWarning($"Duplicate prefab found for type {type}. Skipping {prefab.name}.");
                }
            }
        }

        public GameObject GetPrefab<T>() where T : OverlayBase
        {
            return GetPrefab(typeof(T));
        }

        public GameObject GetPrefab(Type type)
        {
            if (_typeToPrefab == null)
                BuildLookup();

            if (_typeToPrefab.TryGetValue(type, out var prefab))
                return prefab;

            Debug.LogError($"No prefab registered for type {type}");
            return null;
        }
    }
}