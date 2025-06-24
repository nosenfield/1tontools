using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OneTon.Utilities.ScriptableObjectHelpers.EnumGeneration
{
    public class EnumGeneratorPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var allPaths = importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths).Distinct();

            var affectedTypes = new HashSet<Type>();

            foreach (var path in allPaths)
            {
                if (!path.EndsWith(".asset")) continue;

                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType == null || !typeof(ScriptableObject).IsAssignableFrom(assetType)) continue;

                if (Attribute.IsDefined(assetType, typeof(GenerateEnumAttribute)))
                    affectedTypes.Add(assetType);
            }

            foreach (var type in affectedTypes)
                EnumGenerator.GenerateEnumForType(type);
        }
    }
}