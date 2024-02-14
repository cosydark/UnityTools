using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace CommonTools.Bounding
{
    public class BoundUtility
    {
        public static PositionData GetGameObjectVertex(GameObject gameObject) => FetchVertexPositionFromGameObject(gameObject);
        
        public struct PositionData
        {
            public Vector3[] Position;
            public float[] PositionX;
            public float[] PositionY;
            public float[] PositionZ;
        }
        
        // SkinnedMeshRenderer Is Invalid
        private static PositionData FetchVertexPositionFromGameObject(GameObject o)
        {
            List<Vector3> vertices = new List<Vector3>();
            MeshFilter[] meshFilters = o.GetComponentsInChildren<MeshFilter>();
            
            foreach (var meshFilter in meshFilters)
            {
                Vector3[] vertexPositions = meshFilter.sharedMesh.vertices;
                Transform transform = meshFilter.transform;
                vertexPositions = TransformToWorldSpace(vertexPositions, transform);
                vertices.AddRange(vertexPositions);
            }
            // Fill PositionData
            List<float> positionX = new List<float>();
            List<float> positionY = new List<float>();
            List<float> positionZ = new List<float>();
            for (int i = 0; i < vertices.Count; i++)
            {
                positionX.Add(vertices[i].x);
                positionY.Add(vertices[i].y);
                positionZ.Add(vertices[i].z);
            }
            PositionData data = new PositionData
            {
                Position = vertices.ToArray(),
                PositionX = positionX.ToArray(),
                PositionY = positionY.ToArray(),
                PositionZ = positionZ.ToArray()
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
        // Matrix Calculate
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



namespace XPCG.Tools
{
    
    public class CommonBoundBox
    {
        private struct Position
        {
            public Vector3 Pos;
        }
        private struct Output
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
        
        public static Vector3[] ComputeOBB(GameObject g)
        {
            float[] pointsPositionX = new float[1];
            float[] pointsPositionY = new float[1];
            float[] pointsPositionZ = new float[1];
            Vector3[] vertices = GetPoints(g, ref pointsPositionX, ref pointsPositionY, ref pointsPositionZ);
            
            // Vector3 center = vertices.Aggregate(Vector3.zero, (current, t) => current + t);
            Vector3 center = GetCenter(vertices);
            center /= vertices.Length;

            Matrix4x4 cov = Matrix4x4.zero;
            int num = vertices.Length;
            
            for (int i = 0; i < num; i++)
            {
                Vector3 p = vertices[i] - center;
                cov = MatrixAdd(cov, OuterProduct(p, p));
            }
            cov = MatrixDivideFloat(cov, num);
            //
            ComputeShader cs = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.funplus.worldx.pcg/Editor/CommonTool/BB/ComputeWithOBB.compute");
            int kernel = cs.FindKernel("ComputeSVD3D");
            
            GraphicsBuffer output = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, Marshal.SizeOf(typeof(Output)));
            GraphicsBuffer position = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num, Marshal.SizeOf(typeof(Position)));
            
            Output[] outputs = new Output[1];
            output.SetData(outputs); position.SetData(vertices);
            cs.SetVector("_COV0", cov.GetColumn(0));
            cs.SetVector("_COV1", cov.GetColumn(1));
            cs.SetVector("_COV2", cov.GetColumn(2));
            cs.SetVector("_COV3", cov.GetColumn(3));
            cs.SetVector("_Center", center);
            cs.SetInt("_PointCount", num);
            cs.SetBuffer(kernel, "_output_buffer", output);
            cs.SetBuffer(kernel, "_position_buffer", position);
            cs.Dispatch(kernel, 1, 1, 1);
            
            Output[] result = new Output[1];
            output.GetData(result);
            //
            Vector3[] corner = new Vector3[9];
            corner[0] = result[0].P0;
            corner[1] = result[0].P1;
            corner[2] = result[0].P2;
            corner[3] = result[0].P3;
            corner[4] = result[0].P4;
            corner[5] = result[0].P5;
            corner[6] = result[0].P6;
            corner[7] = result[0].P7;
            
            center = (corner[0] + corner[1] + corner[2] + corner[3] + corner[4] + corner[5] + corner[6] + corner[7]) / 8;
            corner[8] = center;
            
            output.Release(); position.Release();
            return corner;
        }

        private static Vector3[] GetPoints(GameObject g, ref float[] x, ref float[] y, ref float[] z)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<float> pointsPositionX = new List<float>();
            List<float> pointsPositionY = new List<float>();
            List<float> pointsPositionZ = new List<float>();
            
            MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                Vector3[] pos = LocalToWorld(meshFilters[i].sharedMesh.vertices, meshFilters[i].transform);
                for (int j = 0; j < pos.Length; j++)
                {
                    // if (vertices.Contains(pos[j])) { continue; } // 重复就重复了，Contains耗的一批
                    vertices.Add(pos[j]);
                    pointsPositionX.Add(pos[j].x);
                    pointsPositionY.Add(pos[j].y);
                    pointsPositionZ.Add(pos[j].z);
                }
            }
            
            SkinnedMeshRenderer[] skinnedMesh = g.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < skinnedMesh.Length; i++)
            {
                Vector3[] pos = LocalToWorld(skinnedMesh[i].sharedMesh.vertices, skinnedMesh[i].transform);
                for (int j = 0; j < pos.Length; j++)
                {
                    // if (vertices.Contains(pos[j])) { continue; } // 重复就重复了，Contains耗的一批
                    vertices.Add(pos[j]);
                    pointsPositionX.Add(pos[j].x);
                    pointsPositionY.Add(pos[j].y);
                    pointsPositionZ.Add(pos[j].z);
                }
            }

            x = pointsPositionX.ToArray();
            y = pointsPositionY.ToArray();
            z = pointsPositionZ.ToArray();
            return vertices.ToArray();
        }

        private static Vector3 GetCenter(Vector3[] vs)
        {
            List<float> pointsPositionX = new List<float>();
            List<float> pointsPositionY = new List<float>();
            List<float> pointsPositionZ = new List<float>();
            
            Vector3[] pos = vs;
            for (int j = 0; j < pos.Length; j++)
            {
                pointsPositionX.Add(pos[j].x);
                pointsPositionY.Add(pos[j].y);
                pointsPositionZ.Add(pos[j].z);
            }
            
            Vector3 min = new Vector3(pointsPositionX.Min(), pointsPositionY.Min(), pointsPositionZ.Min());
            Vector3 max = new Vector3(pointsPositionX.Max(), pointsPositionY.Max(), pointsPositionZ.Max());
        
            Vector3[] corner = new Vector3[8];
            corner[0] = new Vector3(min.x, min.y, min.z);
            corner[1] = new Vector3(min.x, min.y, max.z);
            corner[2] = new Vector3(max.x, min.y, max.z);
            corner[3] = new Vector3(max.x, min.y, min.z);
            corner[4] = new Vector3(min.x, max.y, min.z);
            corner[5] = new Vector3(min.x, max.y, max.z);
            corner[6] = new Vector3(max.x, max.y, max.z);
            corner[7] = new Vector3(max.x, max.y, min.z);
            
            Vector3 center = (corner[0] + corner[1] + corner[2] + corner[3] + corner[4] + corner[5] + corner[6] + corner[7]) / 8;
            return center;
        }
        private static Vector3[] LocalToWorld(Vector3[] p, Transform t)
        {
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = t.TransformPoint(p[i]);
            }
            return p;
        }
        private static Matrix4x4 OuterProduct(Vector4 u, Vector4 v)
        {
            Vector4 r0 = new Vector4(u.x * v.x, u.x * v.y, u.x * v.z, u.x * v.w);
            Vector4 r1 = new Vector4(u.y * v.x, u.y * v.y, u.y * v.z, u.y * v.w);
            Vector4 r2 = new Vector4(u.z * v.x, u.z * v.y, u.z * v.z, u.z * v.w);
            Vector4 r3 = new Vector4(u.w * v.x, u.w * v.y, u.w * v.z, u.w * v.w);
            return new Matrix4x4(r0, r1, r2, r3);
        }
        private static Matrix4x4 MatrixAdd(Matrix4x4 m, Matrix4x4 n)
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
        private static Matrix4x4 MatrixDivideFloat(Matrix4x4 m, float n)
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
        public static void DrawBB(Vector3[] p)
        {
            Handles.color = Color.white;
            Handles.DrawLine(p[0], p[1]);
            Handles.DrawLine(p[1], p[2]);
            Handles.DrawLine(p[2], p[3]);
            Handles.DrawLine(p[3], p[0]);
            
            Handles.DrawLine(p[0], p[4]);
            Handles.DrawLine(p[3], p[7]);
            Handles.DrawLine(p[2], p[6]);
            Handles.DrawLine(p[1], p[5]);
            
            Handles.DrawLine(p[4], p[5]);
            Handles.DrawLine(p[5], p[6]);
            Handles.DrawLine(p[6], p[7]);
            Handles.DrawLine(p[7], p[4]);
            
            Handles.color = Color.blue;
            Handles.DrawLine(p[8], ((p[1] + p[2]) / 2 + (p[5] + p[6]) / 2) / 2);
            Handles.color = Color.green;
            Handles.DrawLine(p[8], ((p[4] + p[7]) / 2 + (p[5] + p[6]) / 2) / 2);
            Handles.color = Color.red;
            Handles.DrawLine(p[8], ((p[3] + p[2]) / 2 + (p[7] + p[6]) / 2) / 2);
        }

        public static Vector3 GetBBSize(Vector3[] p)
        {
            List<float> size = new List<float>
            {
                Vector3.Distance(p[8], ((p[1] + p[2]) / 2 + (p[5] + p[6]) / 2) / 2),
                Vector3.Distance(p[8], ((p[4] + p[7]) / 2 + (p[5] + p[6]) / 2) / 2),
                Vector3.Distance(p[8], ((p[3] + p[2]) / 2 + (p[7] + p[6]) / 2) / 2)
            };
            size.Sort();
            return new Vector3(size[0] * 2f, size[1] * 2f, size[2] * 2f);
        }
    }
}

