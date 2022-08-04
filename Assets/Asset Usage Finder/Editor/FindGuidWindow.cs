using UnityEngine;
using UnityEditor;

namespace Babybus.Evo.AssetFinder.Editor
{
    /// <summary>
    /// 通过GUID查找文件
    /// </summary>
    public class FindGuidWindow : EditorWindow
    {
        private string _guid;
        
        private void OnEnable()
        {
            titleContent = new GUIContent("Find Asset by GUID");
            minSize = new Vector2(380, 40);
            maxSize = new Vector2(380, 40);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("GUID: ", GUILayout.Width(40f));
                _guid = EditorGUILayout.TextField(_guid);
                if (GUILayout.Button("Find", GUILayout.Width(60)))
                    OnClickFind();
            }
        }

        private void OnClickFind()
        {
            if (string.IsNullOrWhiteSpace(_guid))
            {
                EditorUtility.DisplayDialog("Tips", "GUID不能为空，请重新输入", "OK");
                return;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                EditorUtility.DisplayDialog("Tips", "无效的GUID，请重新检查", "OK");
                return;
            }

            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            Debug.Log(assetPath, obj);
            EditorGUIUtility.PingObject(obj);
        }
    }
}