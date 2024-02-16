#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ArtBlockTool
{
    [ExecuteInEditMode]
    public class ArtBlockToolWindow : EditorWindow
    {
        [MenuItem("TA/Art Block Tool")]
        public static void OpenWindow()
        {
            var window = ScriptableObject.CreateInstance<ArtBlockToolWindow>() as ArtBlockToolWindow;
            window.minSize = new Vector2(200, 600);
            window.maxSize = new Vector2(200, 600);
            window.titleContent = new GUIContent("Art Block Tool");
            window.ShowUtility();
        }
        // Options
        private enum SelectMode { Single, Separated}
        
        private bool addWpo;
        private bool addBoxCollider;
        private bool matchSort;
        private bool removeSource;
        private SelectMode mode;
        private int slicedCount = 8;
        private void OnGUI()
        {
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Common Setting", AddColor(Color.green));
            EditorGUILayout.Space(2);
            mode = (SelectMode)EditorGUILayout.EnumPopup("", mode);
            EditorGUILayout.Space(2);
            addWpo = EditorGUILayout.Toggle("Add WPO", addWpo);
            EditorGUILayout.Space(2);
            addBoxCollider = EditorGUILayout.Toggle("Add Collider", addBoxCollider);
            EditorGUILayout.Space(2);
            matchSort = EditorGUILayout.Toggle("Match Sort", matchSort);
            EditorGUILayout.Space(2);
            removeSource = EditorGUILayout.Toggle("Remove Source", removeSource);
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Common Operating", AddColor(Color.green));
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Creat OBB", GUILayout.Height(37)))
            {
            }
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Creat AABB", GUILayout.Height(37)))
            {
            }
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Slice Setting", AddColor(Color.green));
            EditorGUILayout.Space(2);
            slicedCount = EditorGUILayout.IntField("Sliced Count", slicedCount);
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Generate Sliced AABB", GUILayout.Height(37)))
            {
            }
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Custom Setting", AddColor(Color.green));
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Creat Custom BB Generator", GUILayout.Height(37)))
            {
            }
        }

        private static GUIStyle AddColor(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = color;
            return style;
        }
    }
}
#endif
