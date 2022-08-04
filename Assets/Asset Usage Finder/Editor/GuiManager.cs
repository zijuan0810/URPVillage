using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Babybus.Evo.AssetFinder.Editor
{
    [InitializeOnLoad]
    public class GuiManager : UnityEditor.AssetModificationProcessor
    {
        private const string _version = "v4.0";

        static GuiManager()
        {
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        private static void OnSceneSaved(Scene scene)
        {
        }

        public static string[] OnWillSaveAssets(string[] paths)
        {
            return paths;
        }

        public static void InitCache()
        {
            Globals<CacheManager>.GetOrCreate(() =>
            {
                var res = new CacheManager();
                res.Init();
                return res;
            });
        }

        #region Menu

        [MenuItem("Assets/− Copy Asset GUID", false, 34)]
        private static void FindMenu(MenuCommand command)
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject as GameObject);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                Debug.Log(guid);
                EditorGUIUtility.systemCopyBuffer = guid;
            }
        }

        [MenuItem("Assets/− Find Asset by GUID", false, 35)]
        private static void FindAssetByGUID(MenuCommand command)
        {
            EditorWindow.GetWindow<FindGuidWindow>();
        }

        [MenuItem("Assets/− Find Usages in Project", false, 36)]
        private static void FileMenu(MenuCommand command)
        {
            InitCache();

            var continueFinding = DoYouWantToSaveScene();
            if (!continueFinding) return;

            const string pickupMessage = "Please pick up a file from the project!";

            var selecteds = Selection.objects;
            var type = selecteds.GetType();
            for (int i = 0; i < selecteds.Length; i++)
            {
                var selected = selecteds[i];
                if (selected == null || type == typeof(DefaultAsset) || type == typeof(SceneAsset))
                {
                    EditorUtility.DisplayDialog($"{_version}", $"{pickupMessage}", "Ok");
                    return;
                }

                if (type == typeof(GameObject))
                {
                    var prefabProperties = PrefabUtils.GetPrefabProperties(Selection.activeObject as GameObject);
                    if (prefabProperties.IsPartOfStage || prefabProperties.IsSceneObject)
                    {
                        EditorUtility.DisplayDialog($"{_version}", $"{pickupMessage}", "Ok");
                        return;
                    }
                }

                EditorApplication.ExecuteMenuItem("File/Save Project");
                OpenFileWindow(selected);
            }
        }


        [MenuItem("GameObject/− Find Usages in Scene && Stage", false, -1)]
        public static void SceneOrStageMenu(MenuCommand data)
        {
            InitCache();

            const string message = "Please pick up an object from the scene && stage!";

            var selected = Selection.activeObject;
            if (selected == null || !(selected is GameObject))
            {
                EditorUtility.DisplayDialog($"{_version}", $"{message}", "Ok");
                return;
            }

            var continueFinding = DoYouWantToSaveScene();
            if (!continueFinding) return;

            var prefabProperties = PrefabUtils.GetPrefabProperties(Selection.activeObject as GameObject);
            if (prefabProperties.IsPartOfStage)
                OpenStageWindow(selected, prefabProperties.Path);
            else if (prefabProperties.IsSceneObject)
                OpenSceneWindow(selected, SceneManager.GetActiveScene().path);
            else
                EditorUtility.DisplayDialog($"{_version}", $"{message}", "Ok");
        }

        [MenuItem("CONTEXT/Component/− Find Usages of Component", false, 159)]
        public static void FindReferencesToComponent(MenuCommand data)
        {
            InitCache();

            Object selected = data.context;
            if (!selected) return;

            var continueFinding = DoYouWantToSaveScene();
            if (continueFinding)
            {
                var scenePath = SceneManager.GetActiveScene().path;
                OpenSceneWindow(selected, scenePath);
            }
        }

        private static bool DoYouWantToSaveScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.isDirty || string.IsNullOrEmpty(scene.path))
            {
                var response = EditorUtility.DisplayDialogComplex(
                    title: "Asset Usage Finder v4.0",
                    message: "Current scene is not saved yet!",
                    ok: "Save scene and find usages",
                    cancel: "Cancel usage finding",
                    alt: "Find without saving");
                switch (response)
                {
                    case 0: // ok
                        EditorApplication.ExecuteMenuItem("File/Save");
                        return true;
                    case 1: // cancel
                        return false;
                    case 2: // find without saving
                        return true;
                    default:
                        return true;
                }
            }

            return true;
        }

        #endregion Menu

        #region InitWindow

        public static void OpenFileWindow(Object selected)
        {
            var finder = new FileDependencyFinder(selected);

            var window = ScriptableObject.CreateInstance<FileDependencyWindow>();
            window.InitFinder(finder);
            var p = window.position;
            p.size = DependencyWindow.FinderStyleInstance.Size;
            window.position = p;
            window.Show();
        }

        public static void OpenSceneWindow(Object target, string scenePath)
        {
            var finder = new InSceneDependencyFinder(target, scenePath);
            var window = ScriptableObject.CreateInstance<SceneDependencyWindow>();
            window.InitFinder(finder);
            window.Show();
        }

        private static void OpenStageWindow(Object target, string stagePath)
        {
            var finder = new InStageDependencyFinder(target, stagePath);
            var window = ScriptableObject.CreateInstance<StageDependencyWindow>();
            window.InitFinder(finder);
            window.Show();
        }

        #endregion InitWindow
    }
}