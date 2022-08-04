using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Babybus.Evo.AssetFinder.Editor
{
    public static class AssetFinderUtils
    {
        public static Object[] LoadAllAssetsAtPath(string assetPath)
        {
            // prevents error "Do not use readobjectthreaded on scene objects!"
            return typeof(SceneAsset) == AssetDatabase.GetMainAssetTypeAtPath(assetPath)
                ? new[] { AssetDatabase.LoadMainAssetAtPath(assetPath) }
                : AssetDatabase.LoadAllAssetsAtPath(assetPath);
        }

        public static T FirstOfType<T>() where T : Object
        {
            var typeName = typeof(T).Name;

            var guids = AssetDatabase.FindAssets($"t:{typeName}");
            if (!guids.Any())
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            return guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Select(t => (T)AssetDatabase.LoadAssetAtPath(t, typeof(T))).First();
        }
    }
}