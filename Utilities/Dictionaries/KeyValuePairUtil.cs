using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OneTon.Dictionaries
{
    public static class KeyValuePairUtils
    {
        public static void Set<K,V>(ref this KeyValuePair<K,V> pair,K key, V value)
        {
            pair = new KeyValuePair<K, V>(key, value);
        }

        public static void Add<K, V>(this List<KeyValuePair<K, V>> pairList, K key, V value)
        {
            pairList.Add(new KeyValuePair<K, V>(key, value));
        }
    }
}