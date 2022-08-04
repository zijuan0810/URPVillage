using System;
using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Babybus.Evo.AssetFinder.Editor
{
    [Serializable]
    public class SearchTarget
    {
        public Object Target;
        public Option<Object[]> Nested;
        public Object Root;
        public Scene Scene;
        public UnityEditor.SceneManagement.PrefabStage Stage;
        public string AssetPath;

        public SearchTarget(Object target, FinderMode finderMode, string sceneOrStagePath = null)
        {
            Target = target;
            var path = sceneOrStagePath ?? AssetDatabase.GetAssetPath(Target);
            switch (finderMode)
            {
                case FinderMode.File:
                    Root = AssetDatabase.LoadMainAssetAtPath(path);
                    Nested = AssetFinderUtils.LoadAllAssetsAtPath(path);
                    if (AssetDatabase.GetMainAssetTypeAtPath(path).IsAssignableFrom(typeof(SceneAsset)))
                        Scene = SceneManager.GetSceneByPath(path);
                    break;
                case FinderMode.Scene:
                case FinderMode.Stage:
                    Root = Target;
                    var asset = AssetDatabase.GetAssetPath(target);
                    if (Target is GameObject go)
                    {
                        switch (PrefabUtility.GetPrefabAssetType(go))
                        {
                            case PrefabAssetType.Regular:
                            case PrefabAssetType.Variant:
                            {
                                if (string.IsNullOrEmpty(asset))
                                    Nested = go.GetComponents<Component>(); // prefab instance
                                else
                                    Nested = AssetDatabase.LoadAllAssetsAtPath(asset); // prefab file

                                break;
                            }
                            case PrefabAssetType.Model:
                            {
                                Nested = AssetDatabase.LoadAllAssetsAtPath(asset);
                                break;
                            }
                            case PrefabAssetType.MissingAsset:
                            case PrefabAssetType.NotAPrefab:
                                break;
                        }

                        Stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                        if (finderMode == FinderMode.Scene)
                        {
                            if (string.IsNullOrEmpty(sceneOrStagePath))
                                sceneOrStagePath = go.scene.path;
                            Scene = SceneManager.GetSceneByPath(sceneOrStagePath);
                        }
                    }
                    else if (Target is Component c)
                    {
                        // prefab instance
                        Nested = default;
                        if (finderMode == FinderMode.Scene)
                        {
                            if (string.IsNullOrEmpty(sceneOrStagePath))
                                sceneOrStagePath = c.gameObject.scene.path;
                            Scene = SceneManager.GetSceneByPath(sceneOrStagePath);
                        }
                    }
                    else
                    {
                        Nested = AssetDatabase.LoadAllAssetsAtPath(asset);
                        if (AssetDatabase.GetMainAssetTypeAtPath(path).IsAssignableFrom(typeof(SceneAsset)))
                            Scene = SceneManager.GetSceneByPath(path);
                    }

                    break;
            }

            AssetPath = path;
        }

        public bool Check(Object arg)
        {
            if (arg == null || Target == null) return false;
            if (arg == Target) return true;
            if (!Nested.TryGet(out var n)) return false;

            var length = n.Length;
            for (var i = 0; i < length; i++)
                if (n[i] == arg)
                    return true;

            return false;
        }
    }
}