using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneTon.Dictionaries
{

    [System.Serializable]
    public struct SerializedDictionary<K, V>
    {
        [SerializeField]
        public List<K> listK;
        [SerializeField]
        public List<V> listV;


        private void RefreshLists()
        {
            if (listK == null)
            {
                listK = new List<K>();

            }
            if (listV == null)
            {
                listV = new List<V>();

            }
        }

        public void Add(K key, V value)
        {
            RefreshLists();

            int index = listK.IndexOf(key);

            if (index == -1)
            {
                listK.Add(key);
                listV.Add(value);
            }
            else
            {
                listV[index] = value;
            }
        }

        public void Remove(K key)
        {
            RefreshLists();

            int index = listK.IndexOf(key);

            if (index != -1)
            {
                listK.RemoveAt(index);
                listV.RemoveAt(index);
            }
        }

        public bool Has(K key)
        {
            RefreshLists();
            return listK.Contains(key);
        }

        public V this[K key]
        {
            get
            {
                RefreshLists();

                V value = default(V);
                int foundIndex = listK.IndexOf(key);

                if (foundIndex > -1)
                {
                    value = listV[foundIndex];
                }

                return value;
            }

            set
            {
                RefreshLists();

                int foundIndex = listK.IndexOf(key);
                if (foundIndex > -1)
                {
                    listV[foundIndex] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }
    }
}