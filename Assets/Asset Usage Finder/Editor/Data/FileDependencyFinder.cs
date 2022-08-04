using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Babybus.Evo.AssetFinder.Editor
{
    [Serializable]
    public sealed class FileDependencyFinder : DependencyFinder
    {
        public class Pair
        {
            public string AssetPath;
            public string NicifiedPath;
        }

        public Pair[] ScenePaths;

        public FileDependencyFinder(UnityEngine.Object target)
        {
            Target = new SearchTarget(target, FinderMode.File);
            FindDependencies();
            var path = AssetDatabase.GetAssetPath(Target.Target);
            Title = path;
            TabContent = new GUIContent
            {
                text = target.name,
                image = AssetDatabase.GetCachedIcon(path)
            };
        }

        public override void FindDependencies()
        {
            var files = DependencyFinderEngine.GetFilesThatReference(Target);
            Dependencies = Group(files.Where(f => !(f.Target is SceneAsset)))
                .OrderBy(t => t.LabelContent.text, StringComparer.Ordinal)
                .ToArray();
        }

        public override DependencyFinder Nest(UnityEngine.Object o)
        {
            return new FileDependencyFinder(o) { Parent = this };
        }
    }
}