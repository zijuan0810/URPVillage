using System;
using System.Linq;
using UnityEngine;

namespace Babybus.Evo.AssetFinder.Editor
{
    [Serializable]
    public class AssetDependencies
    {
        public string[] DependencyGuids;
        public string HashString;

        [NonSerialized]
        private Hash128 _hashCache;

        public Hash128 DependencyHash
        {
            get => _hashCache.Equals(default) ? Hash128.Parse(HashString) : _hashCache;
            set
            {
                _hashCache = value;
                HashString = value.ToString();
            }
        }

        public bool Contains(string guid)
        {
            return DependencyGuids.Any(d => StringComparer.InvariantCultureIgnoreCase.Equals(guid, d));
        }
    }
}