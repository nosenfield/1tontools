using System;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Text;
using System.IO;

namespace OneTon.Utilities.ScriptableObjectHelpers.EnumGeneration
{
    public static class EnumGenerator
    {
        public static void GenerateEnumForType(Type type)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(type)) return;

            var attr = type.GetCustomAttribute<GenerateEnumAttribute>();
            if (attr == null) return;

            string enumName = attr.EnumName ?? $"{type.Name}Enum";
            string outputPath = attr.OutputPath ?? $"Assets/Scripts/Generated/Enums/{enumName}.cs";

            GenerateEnum(type, enumName, outputPath);
        }

        private static void GenerateEnum(Type scriptableObjectType, string enumName, string outputPath)
        {
            var guids = AssetDatabase.FindAssets($"t:{scriptableObjectType.Name}");
            var names = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                             .Select(path => AssetDatabase.LoadAssetAtPath(path, scriptableObjectType))
                             .Where(obj => obj != null)
                             .Select(obj => obj.name)
                             .Distinct()
                             .OrderBy(name => name)
                             .Select(Utilities.SanitizeName)
                             .ToList();

            if (names.Count == 0)
            {
                Debug.LogWarning($"[EnumGenerator] No {scriptableObjectType.Name} assets found.");
                return;
            }

            var code = new StringBuilder();
            code.AppendLine($"// Auto-generated enum for {scriptableObjectType.Name}");
            code.AppendLine($"public enum {enumName}");
            code.AppendLine("{");
            foreach (var name in names)
            {
                int hash = Utilities.GetDeterministicHashCode(name);
                code.AppendLine($"    {name} = {hash},");
            }
            code.AppendLine("}");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllText(outputPath, code.ToString());
            AssetDatabase.Refresh();

            Debug.Log($"[EnumGenerator] Generated enum {enumName} with {names.Count} entries.");
        }
    }
}