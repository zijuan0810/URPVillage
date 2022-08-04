using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.Linq;
using UnityEditor.SceneManagement;

namespace Babybus.Evo.AssetFinder.Editor
{
    public class DependencyWindow : EditorWindow
    {
        [SerializeField]
        private DependencyFinder _finder;

        [SerializeField]
        protected FinderMode finderMode;

        private GUIContent _sceneIcon;

        private Vector2 _scrollPos;
        private bool _expandFiles = true;

        private bool _expandScenes = true;

        // private Rect _popupButtonRect;
        private PrevClick _click;
        private List<Action> _delayCallActions;

        public static FinderStyle FinderStyleInstance => Globals<FinderStyle>.GetOrCreate(FinderStyle.FindSelf);

        private void OnEnable()
        {
            _delayCallActions = new List<Action>();
        }

        private void BreadCrumbs()
        {
            var parents = _finder.Parents();
            parents.Reverse();
            var w = 0f;
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.BeginHorizontal();
                for (var i = 0; i < parents.Count; i++)
                {
                    var parent = parents[i];
                    var style = i == 0 ? FinderStyleInstance.TabBreadcrumb0 : FinderStyleInstance.TabBreadcrumb1;

                    var styleWidth = style.CalcSize(parent.TabContent).x;
                    if (w > EditorGUIUtility.currentViewWidth - styleWidth)
                    {
                        w = 0f;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    w += styleWidth;

                    if (i == parents.Count - 1)
                    {
                        var res = GUILayout.Toggle(true, parent.TabContent, style);
                        if (!res)
                            EditorGUIUtility.PingObject(parent.Target.Target);
                    }
                    else if (GUILayout.Button(parent.TabContent, style))
                    {
                        EditorGUIUtility.PingObject(parent.Target.Target);
                        _delayCallActions.Add(() => { InitFinder(parent); });
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void Update()
        {
            if (_delayCallActions.Any())
            {
                _delayCallActions.ForEach(x => x());
                _delayCallActions.Clear();
            }
        }

        private void OnGUI()
        {
            if (_delayCallActions == null || _finder == null || (Event.current != null && Event.current.keyCode == KeyCode.Escape))
            {
                _delayCallActions = new List<Action>();
                _delayCallActions.Add(() => Close());
                return;
            }

            using (new GUILayout.VerticalScope())
            {
                BreadCrumbs();
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                {
                    EditorGUILayout.Space();
                    ShowDependencies(_finder.Dependencies);
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();
        }

        public void InitFinder(DependencyFinder d)
        {
            _finder = d;
            titleContent = new GUIContent($"{GetWindowTitleByFindMode(finderMode)}");
            titleContent.tooltip = _finder.Title;
        }

        private void ShowDependencies(ResultRow[] dependencies)
        {
            var nDeps = dependencies.Length;
            _expandFiles = EditorGUILayout.Foldout(_expandFiles, $"{GetContentByFindMode(finderMode)}: [{nDeps}]");

            if (finderMode == FinderMode.File)
            {
                if (_finder.Target.Scene.IsValid() && !_finder.Target.Scene.isLoaded)
                    return;
            }

            if (_expandFiles)
            {
                if (nDeps > 0)
                {
                    foreach (var dependency in dependencies)
                    {
                        if (dependency?.SerializedObject != null && dependency.SerializedObject.targetObject != null)
                            DrawRow(dependency);
                        else
                            Close();
                    }
                }
                else
                    EditorGUILayout.LabelField("No file dependencies found.");
            }

            EditorGUILayout.Space();

            if (!(_finder is FileDependencyFinder fileDep))
                return;
            
            if (fileDep.ScenePaths == null)
            {
                fileDep.ScenePaths = DependencyFinderEngine.GetScenesThatContain(_finder.Target.Target).Select(p =>
                    new FileDependencyFinder.Pair { AssetPath = p, NicifiedPath = p.Replace("Assets/", string.Empty) }).ToArray();
            }
            
            var nScenes = fileDep.ScenePaths.Length;
            _expandScenes = EditorGUILayout.Foldout(_expandScenes, $"In Scenes: [{nScenes}]");
            if (!_expandScenes)
                return;

            if (nScenes > 0)
            {
                foreach (var p in fileDep.ScenePaths)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        SceneIcon.text = p.NicifiedPath;
            
                        // if (GUILayout.Button(SceneIcon, EditorStyles.label, GUILayout.Height(16f),
                        //     GUILayout.MaxWidth(20f + p.NicifiedPath.Length * 7f)))
                        //     EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(p.AssetPath));
                        
                        if (GUILayout.Button(SceneIcon, FinderStyleInstance.RowMainAssetBtn, GUILayout.Height(16f)))
                            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(p.AssetPath));
            
                        if (!GUILayout.Button("    Open scene & search", FinderStyleInstance.RowMainAssetBtn, GUILayout.Height(16f), 
                            GUILayout.MaxWidth(140f)))
                            continue;
            
                        var sceneToOpen = SceneManager.GetSceneByPath(p.AssetPath);
                        if (sceneToOpen.isLoaded)
                            GuiManager.OpenSceneWindow(_finder.Target.Target, p.AssetPath);
                        else
                        {
                            var currentScene = SceneManager.GetActiveScene();
                            if (currentScene.isDirty && EditorUtility.DisplayDialog(
                                $"Unsaved changes",
                                $"You are going to open and search in scene [{p.AssetPath}]\n" +
                                $"but you have unsaved changes at the scene [{currentScene.name}]",
                                $"Stay at current scene and cancel search", $"Discard changes and search"))
                                return;
            
                            EditorSceneManager.OpenScene(p.AssetPath);
                            GuiManager.OpenSceneWindow(_finder.Target.Target, p.AssetPath);
                        }
                    }
                }
            }
            else
                EditorGUILayout.LabelField("No scene dependencies found.");
        }

        public static string GetWindowTitleByFindMode(FinderMode finderMode)
        {
            switch (finderMode)
            {
                case FinderMode.File:
                    return "Usages in Project";
                case FinderMode.Scene:
                    return "Usages in Scene";
                case FinderMode.Stage:
                    return "Usages in Stage";
                default:
                    return "Unknown Title!";
            }
        }

        public static string GetContentByFindMode(FinderMode finderMode)
        {
            switch (finderMode)
            {
                case FinderMode.File:
                    return "In Project Files";
                case FinderMode.Scene:
                    return "In Current Scene";
                case FinderMode.Stage:
                    return "In Current Stage";
                default:
                    return "Unknown Content!";
            }
        }

        private struct PrevClick
        {
            private Object _target;
            private float _timeClicked;
            private const float DoubleClickTime = 0.5f;

            public PrevClick(Object target)
            {
                _target = target;
                _timeClicked = Time.realtimeSinceStartup;
            }

            public bool IsDoubleClick(Object o)
            {
                return _target == o && Time.realtimeSinceStartup - _timeClicked < DoubleClickTime;
            }
        }

        private void DrawRow(ResultRow dependency)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(dependency.LabelContent, FinderStyleInstance.RowMainAssetBtn))
                    {
                        if (_click.IsDoubleClick(dependency.Main))
                            Selection.activeObject = dependency.Main;
                        else
                            EditorGUIUtility.PingObject(dependency.Main);

                        _click = new PrevClick(dependency.Main);
                    }

                    if (GUILayout.Button(FinderStyleInstance.LookupBtn.Content, FinderStyleInstance.LookupBtn.Style))
                        _delayCallActions.Add(() => InitFinder(_finder.Nest(dependency.Main)));
                }

                dependency.SerializedObject.Update();
                
                // EditorGUI.BeginChangeCheck();
                // if (dependency.Target)
                // {
                //     foreach (var prop in dependency.Properties)
                //     {
                //         using (new EditorGUILayout.HorizontalScope())
                //         {
                //             var f = GUI.enabled;
                //             var locked = prop.Property.objectReferenceValue is MonoScript;
                //             if (locked) GUI.enabled = false;
                //
                //             EditorGUILayout.LabelField(prop.Content, FinderStyleInstance.RowLabel,
                //                 GUILayout.MaxWidth(position.width * .8f));
                //             EditorGUILayout.PropertyField(prop.Property, GUIContent.none, true,
                //                 GUILayout.MinWidth(position.width * .2f));
                //
                //             if (locked) GUI.enabled = f;
                //         }
                //     }
                // }
                //
                // if (EditorGUI.EndChangeCheck())
                //     dependency.SerializedObject.ApplyModifiedProperties();
            }
        }

        private GUIContent SceneIcon => _sceneIcon ?? (_sceneIcon = new GUIContent(AssetPreview.GetMiniTypeThumbnail(typeof(SceneAsset))));
    }
}