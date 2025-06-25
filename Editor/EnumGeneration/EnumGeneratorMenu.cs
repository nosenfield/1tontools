using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OneTon.EnumGeneration
{
    public static class EnumGenerationMenu
    {
        [MenuItem("Tools/Enum Generator/Regenerate All")]
        public static void RegenerateAllEnums()
        {
            var typesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.FullName.StartsWith("Unity"))
                .SelectMany(asm => asm.GetTypes())
                .Where(t => t.IsClass && typeof(ScriptableObject).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<GenerateEnumAttribute>() != null)
                .ToList();

            if (typesWithAttribute.Count == 0)
            {
                Debug.LogWarning("[EnumGenerator] No types found with [GenerateEnum] attribute.");
                return;
            }

            foreach (var type in typesWithAttribute)
            {
                EnumGenerator.GenerateEnumForType(type);
            }

            Debug.Log($"[EnumGenerator] Regenerated enums for {typesWithAttribute.Count} types.");
        }
    }
}