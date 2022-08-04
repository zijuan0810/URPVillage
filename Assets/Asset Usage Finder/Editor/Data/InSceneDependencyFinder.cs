using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Babybus.Evo.AssetFinder.Editor
{
    [Serializable]
    internal sealed class InSceneDependencyFinder : DependencyFinder
    {
        [SerializeField]
        private string _scenePath;

        public InSceneDependencyFinder(Object target, string scenePath)
        {
            Target = new SearchTarget(target, FinderMode.Scene, scenePath);
            _scenePath = scenePath;
            Title = scenePath;

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
            var dependenciesInScene = DependencyFinderEngine.GetDependenciesInScene(Target);
            Dependencies = Group(dependenciesInScene).ToArray();
        }

        public override DependencyFinder Nest(Object o)
        {
            return new InSceneDependencyFinder(o, _scenePath) { Parent = this };
        }
    }
}