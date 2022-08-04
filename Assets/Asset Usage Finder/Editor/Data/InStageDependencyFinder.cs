using System;
using System.Linq;
using UnityEditor;

using UnityEngine;
using Object = UnityEngine.Object;

namespace Babybus.Evo.AssetFinder.Editor
{
    [Serializable]
    internal sealed class InStageDependencyFinder : DependencyFinder
    {
        [SerializeField]
        private string _stagePath;

        [SerializeField]
        private UnityEditor.SceneManagement.PrefabStage _stage;

        public InStageDependencyFinder(Object target, string stagePath)
        {
            Target = new SearchTarget(target, FinderMode.Stage, stagePath);
            _stagePath = stagePath;
            _stage = Target.Stage;
            Title = stagePath;

            var name = target is Component ? target.GetType().Name : target.name;

            TabContent = new GUIContent
            {
                text = name,
                image = AssetPreview.GetMiniTypeThumbnail(Target.Target.GetType()) ?? AssetPreview.GetMiniThumbnail(Target.Target)
            };

            FindDependencies();
        }

        public override void FindDependencies()
        {
            Dependencies = Group(DependencyFinderEngine.GetDependenciesInStage(Target, _stage)).ToArray();
        }

        public override DependencyFinder Nest(Object o)
        {
            return new InStageDependencyFinder(o, _stagePath) { Parent = this };
        }
    }
}