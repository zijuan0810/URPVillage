using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Babybus.Evo.AssetFinder.Editor
{
    internal class WindowStyleAsset : ScriptableObject
    {
#pragma warning disable 0649
        public FinderStyle Pro;
        public FinderStyle Personal;
#pragma warning restore

#if !false
        [CustomEditor(typeof(WindowStyleAsset))]
        private class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();
                base.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                    InternalEditorUtility.RepaintAllViews();
            }
        }
#endif
    }
}