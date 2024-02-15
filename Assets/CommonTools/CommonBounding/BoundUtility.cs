using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace CommonTools.Bounding
{
    public abstract class BoundUtility
    {
        public static PositionData GetGameObjectConvexHull(GameObject gameObject) => FetchConvexHullPointsFromGameObject(gameObject);
        public static PositionData GetGameObjectVertex(GameObject gameObject) => FetchVertexPositionFromGameObject(gameObject);
        
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
        
        #endregion
        
        #region PrivateFunction
        
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