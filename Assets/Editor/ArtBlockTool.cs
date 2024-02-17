#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using CommonTools.Bounding;
using UnityEngine;
using UnityEditor;

namespace ArtBlockTool
{
    [ExecuteInEditMode]
    public class ArtBlockToolWindow : EditorWindow
    {
        [MenuItem("ARK/Art Block Tool")]
        public static void OpenWindow()
        {
            var window = ScriptableObject.CreateInstance<ArtBlockToolWindow>() as ArtBlockToolWindow;
            window.minSize = new Vector2(200, 600);
            window.maxSize = new Vector2(200, 600);
            window.titleContent = new GUIContent("Art Block Tool");
            window.ShowUtility();
        }
        // Options
        private enum BoundMode { OrientedBoundingBox, AxisAlignedBoundingBox, SmallestBoundingSphere}
        
        private bool addWpo;
        private bool addBoxCollider = true;
        private bool matchSort;
        private bool removeSource;
        private string fbxPath = "FBX Path";
        private string prefabPath = "Prefab Path";
        private void OnGUI()
        {
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Art Block Post Processing", AddColor(Color.green));
            EditorGUILayout.Space(2);
            addWpo = EditorGUILayout.Toggle("Add WPO", addWpo);
            EditorGUILayout.Space(2);
            addBoxCollider = EditorGUILayout.Toggle("Add Collider", addBoxCollider);
            EditorGUILayout.Space(2);
            matchSort = EditorGUILayout.Toggle("Match Sort", matchSort);
            EditorGUILayout.Space(2);
            removeSource = EditorGUILayout.Toggle("Remove Source", removeSource);
            // myMaterial = (Material)EditorGUILayout.ObjectField("Material", myMaterial, typeof(Material), false);
            // =========================================================================================================
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Common Operating", AddColor(Color.green));
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Creat OBB 3D (Demo)", GUILayout.Height(37)))
            {
                GameObject[] gameObjects = Selection.gameObjects;
                List<Vector3> vertex = FetchAllVertex(gameObjects);
                
                Matrix4x4 trs = OrientedBoundingBox.GetOrientedBoundingBoxTrsMatrix(
                    new BoundUtility.PositionData() { Position = vertex.ToArray() });

                GameObject o = BoundUtility.CreatArtBlockBox(Selection.activeObject.name, trs);
                RemoveSource(gameObjects);
                Selection.activeObject = ArtBlockPostProcessing(o);
            }
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Creat OBB 2D", GUILayout.Height(37)))
            {
                GameObject[] gameObjects = Selection.gameObjects;
                List<Vector3> vertex = FetchAllVertex(gameObjects);
                
                Matrix4x4 trs = OrientedBoundingBox.GetOrientedBoundingBoxTrsMatrix2D(
                    new BoundUtility.PositionData() { Position = vertex.ToArray() });
                
                GameObject o = BoundUtility.CreatArtBlockBox(Selection.activeObject.name, trs);
                RemoveSource(gameObjects);
                Selection.activeObject = ArtBlockPostProcessing(o);
            }
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Creat AABB", GUILayout.Height(37)))
            {
                GameObject[] gameObjects = Selection.gameObjects;
                List<Vector3> vertex = FetchAllVertex(gameObjects);
                
                Matrix4x4 trs = AxisAlignedBoundingBox.GetAxisAlignedBoundingBoxTrsMatrix(
                    new BoundUtility.PositionData() { Position = vertex.ToArray() });
                GameObject o = BoundUtility.CreatArtBlockBox(Selection.activeObject.name, trs);
                
                RemoveSource(gameObjects);
                Selection.activeObject = ArtBlockPostProcessing(o);
            }
            // EditorGUILayout.Space(2);
            // if (GUILayout.Button("Creat SBS (Demo)", GUILayout.Height(37)))
            // {
            //     List<Vector3> vertex = FetchAllVertex(Selection.gameObjects);
            //     Matrix4x4 trs = SmallestBoundingSphere.GetSmallestSphere(vertex.ToArray());
            //     BoundUtility.CreatArtBlockSphere(Selection.activeObject.name, trs);
            // }
            // =========================================================================================================
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Prefab Setting", AddColor(Color.green));
            EditorGUILayout.Space(2);
            fbxPath = EditorGUILayout.TextField("", fbxPath);
            EditorGUILayout.Space(2);
            prefabPath = EditorGUILayout.TextField("", prefabPath);
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Save As New Prefab", GUILayout.Height(37)))
            {
            }
            // =========================================================================================================
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Custom Setting", AddColor(Color.green));
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Creat Custom BB Generator", GUILayout.Height(37)))
            {
            }
        }
        
        #region PrivateFunction

        private void RemoveSource(GameObject[] gameObjects)
        {
            foreach (GameObject obj in gameObjects)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }
        private GameObject ArtBlockPostProcessing(GameObject gameObject)
        {
            if (!addBoxCollider)
            {
                Object.DestroyImmediate(gameObject.GetComponent<BoxCollider>());
            }
            return gameObject;
        }
        private static List<Vector3> FetchAllVertex(GameObject[] gameObjects)
        {
            List<Vector3> vertex = new List<Vector3>();
            GameObject[] validGameObjects = FetchValidGameObject(gameObjects);
            foreach (var t in validGameObjects)
            {
                vertex.AddRange(BoundUtility.GetGameObjectVertex(t).Position);
            }
            return vertex;
        }
        private static GUIStyle AddColor(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = color;
            return style;
        }
        
        private static GameObject[] FetchValidGameObject(GameObject[] gameObjects)
        {
            HashSet<GameObject> result = new HashSet<GameObject>();
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            foreach (var t in gameObjects)
            {
                meshFilters.AddRange(t.GetComponentsInChildren<MeshFilter>());
            }
            
            for (int i = 0; i < meshFilters.Count; i++)
            {
                result.Add(meshFilters[i].gameObject);
            }
            return result.ToArray();
        }
        
        #endregion
        
        #region PublicStruct
        public struct ArtBlock
        {
            public bool Vaild;
            public GameObject Block;
        }
        #endregion
    }
}
#endif
