using UnityEngine;


namespace OneTon.Utilities.ScriptableObjectHelpers
{
    /// <summary>
    /// ScriptableOjectReferences give us a way to address the compile-time dependency of ScriptableObjects
    /// needing to be referenced somewhere within a scene to be included when building the project.
    /// </summary>
    public class ScriptableOjectReferences : MonoBehaviour
    {
        [SerializeField] private ScriptableObject[] objects;
    }
}