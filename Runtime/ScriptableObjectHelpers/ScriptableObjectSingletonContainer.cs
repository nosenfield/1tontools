using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneTon.Utilities.ScriptableObjectHelpers;
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
    [CreateAssetMenu(fileName = "_ScriptableObjectSingletonContainer.asset", menuName = "1Ton/ScriptableObjects/ScriptableObjectSingletonContainer")]
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
    /// - doesn't require code changes in the event we change out mind about using a singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ScriptableObjectSingleton<T> where T : ScriptableObject
    {
        static T _instance = null;
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    List<ScriptableObjectSingletonContainer> containers = new(Resources.FindObjectsOfTypeAll<ScriptableObjectSingletonContainer>());
                    for (int i = containers.Count - 1; i >= 0; i--)
                    {
                        if (containers[i].MasterInstance.GetType() != typeof(T))
                        {
                            containers.RemoveAt(i);
                        }
                    }

                    if (containers.Count == 1)
                    {
                        Debug.Log($"Found one container with instance of type {containers[0].MasterInstance.GetType()}");
                        _instance = containers[0].MasterInstance as T;
                        if (_instance is INeedsInitialization)
                        {
                            (_instance as INeedsInitialization).Initialize();
                        }
                    }
                    else if (containers.Count == 0)
                    {
                        throw new($"ScriptableObjectSingletonContainer with instance of type {typeof(T)} does not exist.");
                    }
                    else if (containers.Count > 1)
                    {
                        throw new($"More than one ScriptableObjectSingletonContainers with instance of type {typeof(T)} exists.");
                    }
                }

                return _instance;
            }
        }

        private ScriptableObjectSingleton() { }
    }
}