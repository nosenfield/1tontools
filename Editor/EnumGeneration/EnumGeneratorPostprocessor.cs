using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OneTon.EnumGeneration
{
    public class EnumGeneratorPostprocessor : AssetPostprocessor
    {
        private static readonly HashSet<string> pendingAssetPaths = new HashSet<string>();

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
                                           string[] movedAssets, string[] movedFromAssetPaths)
        {
            var allPaths = importedAssets.Concat(deletedAssets)
                                         .Concat(movedAssets)
                                         .Concat(movedFromAssetPaths)
                                         .Where(p => p.EndsWith(".asset"));

            foreach (var path in allPaths)
                pendingAssetPaths.Add(path);

            // Defer actual processing to next editor update
            EditorApplication.delayCall += ProcessPendingAssets;
        }

        private static void ProcessPendingAssets()
        {
            var affectedTypes = new HashSet<Type>();

            foreach (var path in pendingAssetPaths)
            {
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType == null || !typeof(ScriptableObject).IsAssignableFrom(assetType))
                    continue;

                if (Attribute.IsDefined(assetType, typeof(GenerateEnumAttribute)))
                    affectedTypes.Add(assetType);
            }

            pendingAssetPaths.Clear();

            foreach (var type in affectedTypes)
                EnumGenerator.GenerateEnumForType(type);
        }
    }
}
