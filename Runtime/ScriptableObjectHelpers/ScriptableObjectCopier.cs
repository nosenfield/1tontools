using UnityEngine;
using System.Reflection;

public static class ScriptableObjectCopier
{
    public static void CopyValues<T>(T source, T destination) where T : ScriptableObject
    {
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(source);
            field.SetValue(destination, value);
        }
    }
}