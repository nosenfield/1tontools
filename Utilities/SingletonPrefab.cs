using Sirenix.OdinInspector;
using UnityEngine;

namespace OneTon.Utilities
{
    [CreateAssetMenu(fileName = "_NewSingletonPrefab.asset", menuName = "nosenfield/Utilities/SingletonPrefab")]
    public class SingletonPrefab : ScriptableObject
    {
        [ReadOnly] public SingletonPrefabMarker Instance;
    }
}