using UnityEngine;

namespace OneTon.Utilities.ScriptableObjectHelpers
{
    /// <summary>
    /// InitializableScriptableObjects are objects which need the equivalent of a consistent OnAwake() call
    /// and can be used in conjunction with the ScriptableObjectReferences script.
    /// </summary>
    public interface INeedsInitialization
    {
        public void Initialize();
    }
}