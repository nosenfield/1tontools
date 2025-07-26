using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OneTon.Utilities
{
    [System.Serializable]
    public struct EnumDictionary<E, V> where E : System.Enum
    {
        [SerializeField]
        public List<E> listE;
        [SerializeField]
        public List<V> listV;


        private void RefreshLists()
        {
            if (listE == null)
            {
                listE = new List<E>();

            }
            if (listV == null)
            {
                listV = new List<V>();
            }
        }

        public void Add(E key, V value)
        {
            RefreshLists();

            int index = listE.IndexOf(key);

            if (index == -1)
            {
                listE.Add(key);
                listV.Add(value);
            }
            else
            {
                listV[index] = value;
            }
        }

        public void Remove(E key)
        {
            RefreshLists();

            int index = listE.IndexOf(key);

            if (index != -1)
            {
                listE.RemoveAt(index);
                listV.RemoveAt(index);
            }
        }

        public bool Has(E key)
        {
            RefreshLists();
            return listE.Contains(key);
        }

        public V this[E key]
        {
            get
            {
                RefreshLists();

                V value = default(V);
                int foundIndex = listE.IndexOf(key);

                if (foundIndex > -1)
                {
                    value = listV[foundIndex];
                }

                return value;
            }

            set
            {
                RefreshLists();

                int foundIndex = listE.IndexOf(key);
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