using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OneTon.SceneManagement
{
    /*
    // A SceneCollection asset is used to define a group of Unity scenes that can be loaded simultaneously by the StateManager.
    // The StateManager takes this asset as 
    */
    [CreateAssetMenu(fileName = "_NewSceneCollection.asset", menuName = "nosenfield/SceneManagement/SceneCollection")]
    public class SceneCollection : ScriptableObject
    {
        public SceneReference[] Scenes;
        public SceneCollection[] Subcollections;
    }
}