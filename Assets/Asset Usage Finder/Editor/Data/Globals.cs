using System;

namespace Babybus.Evo.AssetFinder.Editor
{
    public static class Globals<T> where T : class
    {
        private static T _instance;

        public static void TryInit(Func<T> ctor)
        {
            if (_instance == null)
                _instance = ctor.Invoke();
        }

        public static T Get() => _instance;

        public static T GetOrCreate(Func<T> ctor)
        {
            TryInit(ctor);
            return _instance;
        }
    }
}