using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.BoundingBox
{
    public class AxisAlignedBoundingBox
    {
        // Public Functions
        public List<Vector3> GetAxisAlignedBoundingBox(GameObject gameObject) => ComputeGameObjectAABB_Box(gameObject);
        public Vector3 GetAxisAlignedBoundingCenter(GameObject gameObject) => ComputeGameObjectAABB_Center(gameObject);
        public Vector3 GetAxisAlignedBoundingCenterBot(GameObject gameObject) => ComputeGameObjectAABB_CenterBot(gameObject);
        public Vector3 GetAxisAlignedBoundingSize(GameObject gameObject) => ComputeGameObjectAABB_Size(gameObject);
        
        
        private List<Vector3> ComputeGameObjectAABB_Box(GameObject o)
        {
            Vector3[] corners = ComputeAABBCorners(o);
            List<Vector3> box = new List<Vector3>();
            for (int i = 0; i < 8; i++)
            {
                box.Add(corners[i]);
            }
            return box;
        }

        private Vector3 ComputeGameObjectAABB_Center(GameObject o)
        {
            return ComputeAABBCorners(o)[8];
        }
        
        private Vector3 ComputeGameObjectAABB_CenterBot(GameObject o)
        {
            return ComputeAABBCorners(o)[9];
        }
        
        private Vector3 ComputeGameObjectAABB_Size(GameObject o)
        {
            return ComputeAABBCorners(o)[10];
        }
        
        /// <summary>
        /// 0 - 7 => Eight Corner, 8 => Center, 9 => Bottom Center, 10 => Size
        /// </summary>
        private Vector3[] ComputeAABBCorners(GameObject o)
        {
            var data = FetchVertexPositionFromGameObject(o);
            Vector3 min = new Vector3(data.PositionX.Min(), data.PositionY.Min(), data.PositionZ.Min());
            Vector3 max = new Vector3(data.PositionX.Max(), data.PositionY.Max(), data.PositionZ.Max());
        
            Vector3[] corner = new Vector3[10];
            // Eight Corner
            corner[0] = new Vector3(min.x, min.y, min.z);
            corner[1] = new Vector3(min.x, min.y, max.z);
            corner[2] = new Vector3(max.x, min.y, max.z);
            corner[3] = new Vector3(max.x, min.y, min.z);
            corner[4] = new Vector3(min.x, max.y, min.z);
            corner[5] = new Vector3(min.x, max.y, max.z);
            corner[6] = new Vector3(max.x, max.y, max.z);
            corner[7] = new Vector3(max.x, max.y, min.z);
            // Average Is Center
            Vector3 center = (corner[0] + corner[1] + corner[2] + corner[3] + corner[4] + corner[5] + corner[6] + corner[7]) / 8;
            corner[8] = center;
            // Center Bot
            corner[9] = new Vector3(center.x, min.y, center.z);
            // Size
            corner[10] = new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), Mathf.Abs(max.z - min.z));
            return corner;
        }
        // Utility
        
        // SkinnedMeshRenderer Is Not Invalid
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
        
        private struct PositionData
        {
            public Vector3[] Position;
            public float[] PositionX;
            public float[] PositionY;
            public float[] PositionZ;
        }
        
        private static Vector3[] TransformToWorldSpace(Vector3[] positions, Transform transform)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = transform.TransformPoint(positions[i]);
            }
            return positions;
        }
    }
}

