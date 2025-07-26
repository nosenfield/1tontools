using UnityEngine;

namespace OneTon.ScriptableObjectHelpers
{
    /// <summary>
    /// Implement INeedsInitialization to perform a one-time setup function the first time your SomeClass : ScriptableObject
    /// is referenced via ScriptableObjectSingleton<SomeClass>. 
    /// </summary>
    public interface INeedsInitialization
    {
        public void Initialize();
    }
}