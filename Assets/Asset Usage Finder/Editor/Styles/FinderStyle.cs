using System;
using UnityEngine;
using UnityEditor;

namespace Babybus.Evo.AssetFinder.Editor
{
    [Serializable]
    public class ContentStylePair
    {
        public GUIStyle Style = new GUIStyle();
        public GUIContent Content = new GUIContent();
    }
    
    [Serializable]
    public class FinderStyle
    {
        public ContentStylePair LookupBtn = new ContentStylePair();
        public GUIStyle TabBreadcrumb0 = new GUIStyle();
        public GUIStyle TabBreadcrumb1 = new GUIStyle();
        public GUIStyle RowMainAssetBtn = new GUIStyle();
        public Vector2 Size = new Vector2(250f, 800f);
        public GUIStyle RowLabel = new GUIStyle();

        public static FinderStyle FindSelf()
        {
            var res = AssetFinderUtils.FirstOfType<WindowStyleAsset>();
            return EditorGUIUtility.isProSkin ? res.Pro : res.Personal;
        }
    }
}