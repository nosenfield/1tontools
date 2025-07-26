using System.Collections.Generic;
using UnityEngine;

namespace OneTon.Utilities
{
    /// <summary>
    /// RuntimeSet is a universally accessible way for storing a list of objects.
    /// 
    /// Taken from this talk on ScriptableObjects https://www.youtube.com/watch?v=raQ3iHhE_Kk
    /// To use, extend this class and add the appopriate menu creation option:
    /// 
    /// [CreateAssetMenu(fileName = "_DefaultAssetName.asset", menuName = "Path/To/MenuOption")]
    /// 
    /// Scripts of type SomeClass hold a reference to the RuntimeSet<SomeClass> and add themselves to the set as needed.
    /// This might be in Awake() or later on in response to some event/condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RuntimeSet<T> : ScriptableObject
    {
        public List<T> Items = new List<T>();
        public void Add(T t)
        {
            if (!Items.Contains(t)) Items.Add(t);
        }

        public void Remove(T t)
        {
            if (Items.Contains(t)) Items.Remove(t);
        }
    }
}