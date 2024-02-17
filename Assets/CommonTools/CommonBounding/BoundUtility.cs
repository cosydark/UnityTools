using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace CommonTools.Bounding
{
    public abstract class BoundUtility
    {
        public static PositionData GetGameObjectConvexHull(GameObject gameObject) => FetchConvexHullPointsFromGameObject(gameObject);
        public static PositionData GetGameObjectVertex(GameObject gameObject) => FetchVertexPositionFromGameObject(gameObject);
        public static PositionData RegeneratePositionData(PositionData data) => FillPositionDataFromPosition(data);
        public static GameObject CreatArtBlockBox(string name, Matrix4x4 trs) => CreatArtBlockFromTrsMatrix_Box(name, trs);
        public static GameObject CreatArtBlockSphere(string name, Matrix4x4 trs) => CreatArtBlockFromTrsMatrix_Sphere(name, trs);
        
        #region PublicStruct
        
        /// <summary>
        /// PositionData Should Init From Hashset
        /// </summary>
        public struct PositionData
        {
            public Vector3[] Position;
            public float[] PositionX;
            public float[] PositionY;
            public float[] PositionZ;
        }
        
        public struct PositionBuffer
        {
            public Vector3 VertexPosition;
        }
        public struct BoundingBoxBuffer
        {
            public Vector4 P0;
            public Vector4 P1;
            public Vector4 P2;
            public Vector4 P3;
            public Vector4 P4;
            public Vector4 P5;
            public Vector4 P6;
            public Vector4 P7;
        }
        
        
        
        #endregion
        
        #region PrivateFunction
        
        // SkinnedMeshRenderer Is Invalid
        private static PositionData FetchConvexHullPointsFromGameObject(GameObject o)
        {
            PositionData data = FetchVertexPositionFromGameObject(o);
            Vector3[] hull = ConvexHullFunctions.ConvexHull.GenerateCorners(data.Position);
            data = new PositionData
            {
                Position = hull,
                PositionX = hull.Select(v => v.x).ToArray(),
                PositionY = hull.Select(v => v.y).ToArray(),
                PositionZ = hull.Select(v => v.z).ToArray()
            };
            return data;
        }
        
        private static GameObject CreatArtBlockFromTrsMatrix_Sphere(string name, Matrix4x4 trs)
        {
            if (!trs.ValidTRS()) { return null; }
            GameObject artBlock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            artBlock.name = $"{name} SBS";
            artBlock.transform.position = trs.GetT();
            artBlock.transform.localScale = trs.GetS();
            return artBlock;
        }
        
        private static GameObject CreatArtBlockFromTrsMatrix_Box(string name, Matrix4x4 trs)
        {
            if (!trs.ValidTRS()) { return null; }
            GameObject artBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(artBlock, "Create " + artBlock.name);
            artBlock.name = $"{name} OBB";
            artBlock.transform.position = trs.GetT();
            artBlock.transform.rotation = trs.GetR();
            artBlock.transform.localScale = trs.GetS();
            return artBlock;
        }
        
        private static PositionData FillPositionDataFromPosition(PositionData data)
        {
            PositionData newData = new PositionData();
            List<Vector3> newPosition = new List<Vector3>();
            List<float> newPositionX = new List<float>();
            List<float> newPositionY = new List<float>();
            List<float> newPositionZ = new List<float>();
            for (int i = 0; i < data.Position.Length; i++)
            {
                newPosition.Add(data.Position[i]);
                newPositionX.Add(data.Position[i].x);
                newPositionY.Add(data.Position[i].y);
                newPositionZ.Add(data.Position[i].z);
            }

            newData.Position = newPosition.ToArray();
            newData.PositionX = newPositionX.ToArray();
            newData.PositionY = newPositionY.ToArray();
            newData.PositionZ = newPositionZ.ToArray();
            return newData;
        }
        // SkinnedMeshRenderer Is Invalid
        private static PositionData FetchVertexPositionFromGameObject(GameObject o)
        {
            HashSet<Vector3> vertexHash = new HashSet<Vector3>();
            MeshFilter[] meshFilters = o.GetComponentsInChildren<MeshFilter>();

            foreach (var meshFilter in meshFilters)
            {
                Vector3[] vertexPositions = meshFilter.sharedMesh.vertices;
                Transform transform = meshFilter.transform;
                vertexPositions = TransformToWorldSpace(vertexPositions, transform);
                for (int i = 0; i < vertexPositions.Length; i++)
                {
                    vertexHash.Add(vertexPositions[i]);
                }
            }
            // Fill
            Vector3[] vertexList = vertexHash.ToArray();
            PositionData data = new PositionData
            {
                Position = vertexList,
                PositionX = vertexList.Select(v => v.x).ToArray(),
                PositionY = vertexList.Select(v => v.y).ToArray(),
                PositionZ = vertexList.Select(v => v.z).ToArray()
            };
            return data;
        }
        
        private static Vector3[] TransformToWorldSpace(Vector3[] positions, Transform transform)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = transform.TransformPoint(positions[i]);
            }
            return positions;
        }
        
        #endregion
    }

    public abstract class BoundMath
    {
        public static Matrix4x4 MatrixAddFloat(Matrix4x4 m, Matrix4x4 n)
        {
            Matrix4x4 result = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = m[i, j] + n[i, j];
                }
            }

            return result;
        }

        public static Matrix4x4 OuterProduct(Vector4 u, Vector4 v)
        {
            Vector4 r0 = new Vector4(u.x * v.x, u.x * v.y, u.x * v.z, u.x * v.w);
            Vector4 r1 = new Vector4(u.y * v.x, u.y * v.y, u.y * v.z, u.y * v.w);
            Vector4 r2 = new Vector4(u.z * v.x, u.z * v.y, u.z * v.z, u.z * v.w);
            Vector4 r3 = new Vector4(u.w * v.x, u.w * v.y, u.w * v.z, u.w * v.w);
            return new Matrix4x4(r0, r1, r2, r3);
        }

        public static Matrix4x4 MatrixDivideFloat(Matrix4x4 m, float n)
        {
            Matrix4x4 result = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = m[i, j] / n;
                }
            }

            return result;
        }
    }
}