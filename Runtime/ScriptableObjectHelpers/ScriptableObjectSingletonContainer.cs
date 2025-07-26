using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Abstract class for making reload-proof singletons out of ScriptableObjects
/// Returns the asset created on the editor, or creates one if there is none
/// Based on https://www.youtube.com/watch?v=VBA1QCoEAX4
/// </summary>

namespace OneTon.ScriptableObjectHelpers
{
    /// <summary>
    /// ScriptableObjectSingletonContainer acts as the instance identifier for a scriptable object wishing to be accessed like a singleton
    /// 
    /// Note: Instances of ScriptableObjectSingletonContainer should be added to the ScriptableObjectReferences script to ensure inclusion at build time
    /// 
    /// Note: This object must be loaded into RAM this session in order to be found by the ScriptableObjectSingleton<T>.Instance call.
    /// Loading to RAM can be achieved by being referenced in a loaded scene or clicking on the scriptable object in the Project View.
    /// </summary>
    [CreateAssetMenu(fileName = "_ScriptableObjectSingletonContainer.asset", menuName = "1ton/ScriptableObjectHelpers/ScriptableObjectSingletonContainer")]
    public class ScriptableObjectSingletonContainer : ScriptableObject
    {
        [SerializeField] private ScriptableObject masterInstance;
        public ScriptableObject MasterInstance
        {
            get
            {
                return masterInstance;
            }
        }

        void OnValidate()
        {
            Type instanceType = masterInstance.GetType();

            ScriptableObjectSingletonContainer[] containers = Resources.FindObjectsOfTypeAll<ScriptableObjectSingletonContainer>();
            for (int i = 0; i < containers.Length; i++)
            {
                if (containers[i] != this && containers[i].MasterInstance.GetType() == instanceType)
                {
                    Debug.LogWarning($"SingletonContainer for type {instanceType} already exists. Nullifying reference.");
                    masterInstance = null;
                }
            }

            ScriptableObject[] instances = Resources.FindObjectsOfTypeAll(instanceType) as ScriptableObject[];
            if (instances.Length > 1)
            {
                Debug.LogWarning($"Multiple instance of type {instanceType} in project. {masterInstance.name} will be used when calling ScriptableObjectSingleton<{instanceType}>.Instance");
            }
        }
    }

    /// <summary>
    /// ScriptableObjectSingleton enables programmatic access to a master instance of any scriptable object class via ScriptableObjectSingleton<SomeClass>
    /// The purpose is to give a more flexible way to turn a scriptable object into a single accesible instance that:
    /// - doesn't require the calling script to have a reference
    /// - doesn't prevent multiple instantiation of the object in the event we change our mind about using a singleton
    /// - doesn't require code changes in the event we change our mind about using a singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ScriptableObjectSingleton<T> where T : ScriptableObject
    {
        static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // Try to find it in the editor (e.g. after recompilation or domain reload)
                List<ScriptableObjectSingletonContainer> containers = new(Resources.FindObjectsOfTypeAll<ScriptableObjectSingletonContainer>());
                for (int i = containers.Count - 1; i >= 0; i--)
                {
                    if (containers[i].MasterInstance == null || containers[i].MasterInstance.GetType() != typeof(T))
                    {
                        containers.RemoveAt(i);
                    }
                }

                if (containers.Count == 1)
                {
                    Debug.Log($"[Singleton<{typeof(T).Name}>] Found singleton via editor memory or scene reference.");
                    _instance = containers[0].MasterInstance as T;
                }
                else if (containers.Count > 1)
                {
                    throw new($"More than one ScriptableObjectSingletonContainers with instance of type {typeof(T)} exists.");
                }

                // Fallback: load from Resources
                if (_instance == null)
                {
                    string path = $"{typeof(T).Name}_SingletonContainer";
                    var container = Resources.Load<ScriptableObjectSingletonContainer>(path);

                    if (container == null || container.MasterInstance == null)
                        throw new Exception($"[Singleton<{typeof(T).Name}] No ScriptableObjectSingletonContainer found at Resources/{path}.asset");

                    _instance = container.MasterInstance as T;
                    if (_instance == null)
                        throw new Exception($"[Singleton<{typeof(T).Name}] Loaded container's MasterInstance was not of expected type {typeof(T)}.");
                }

                // Optional initialization hook
                if (_instance is INeedsInitialization needsInit)
                {
                    needsInit.Initialize();
                }

                return _instance;
            }
        }

        private ScriptableObjectSingleton() { }
    }
}