using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CommonTools.Bounding
{
    public class OrientedBoundingBox
    {
        public static Vector3[] GetOrientedBoundingBox(GameObject o) => ComputeGameObjectOBB_Box(o);
        public static Matrix4x4 GetOrientedBoundingBoxTrsMatrix(GameObject gameObject) => ComputeGameObjectOBB_TrsMatrix(gameObject);
        public static Matrix4x4 GetOrientedBoundingBoxTrsMatrix(BoundUtility.PositionData data) => ComputeGameObjectOBB_TrsMatrix(data);
        public static Matrix4x4 GetOrientedBoundingBoxTrsMatrix2D(GameObject gameObject) => ComputeGameObjectOBB_TrsMatrix2D(gameObject);
        public static Matrix4x4 GetOrientedBoundingBoxTrsMatrix2D(BoundUtility.PositionData data3D) => ComputeGameObjectOBB_TrsMatrix2D(data3D);
        
        #region PrivateFunctions

        private static Vector3[] ComputeGameObjectOBB_Box(GameObject o)
        {
            var data = BoundUtility.GetGameObjectConvexHull(o);
            return ComputeOBB(data).Take(8).ToArray();
        }
        //     Forward Direction (bot, top)
        //     (2, 3)           (1, 0)
        //      ________________
        //     |                |
        //     |                |
        //     |    Top View    |     Right Direction
        //     |                |
        //     |                |
        //      ________________
        //     (6, 7)           (5, 4)
        private static Matrix4x4 ComputeGameObjectOBB_TrsMatrix(GameObject o)
        {
            var data = BoundUtility.GetGameObjectConvexHull(o);
            var box = ComputeOBB(data);
            // Size Came From Alchemical
            Vector3 size = new Vector3(Vector3.Distance(box[0], box[3]), Vector3.Distance(box[0], box[1]),
                Vector3.Distance(box[0], box[4]));
            Vector3 up = (box[0] - box[1]).normalized;
            Vector3 forward = (box[0] - box[4]).normalized;
            Quaternion rotation = Quaternion.LookRotation(forward, up);
            Vector3 center = box[8];
            return Matrix4x4.TRS(center, rotation, size);
        }
        
        private static Matrix4x4 ComputeGameObjectOBB_TrsMatrix(BoundUtility.PositionData data)
        {
            var box = ComputeOBB(data);
            // Size Came From Alchemical
            Vector3 size = new Vector3(Vector3.Distance(box[0], box[3]), Vector3.Distance(box[0], box[1]),
                Vector3.Distance(box[0], box[4]));
            Vector3 up = (box[0] - box[1]).normalized;
            Vector3 forward = (box[0] - box[4]).normalized;
            Quaternion rotation = Quaternion.LookRotation(forward, up);
            Vector3 center = box[8];
            return Matrix4x4.TRS(center, rotation, size);
        }
        private static Matrix4x4 ComputeGameObjectOBB_TrsMatrix2D(BoundUtility.PositionData data3D)
        {
            // Convert 2D
            BoundUtility.PositionData data2D = new BoundUtility.PositionData();
            List<Vector3> position2D = new List<Vector3>();
            for (int i = 0; i < data3D.Position.Length; i++)
            {
                position2D.Add(new Vector3(data3D.Position[i].x, 0, data3D.Position[i].z));
            }
            data2D.Position = position2D.ToArray();
            data2D = BoundUtility.RegeneratePositionData(data2D);
            data3D = BoundUtility.RegeneratePositionData(data3D);
            // Replace Box
            float BlockYMax = data3D.PositionY.Max();
            float BlockYMin = data3D.PositionY.Min();
            float sizeY = Math.Abs(BlockYMax - BlockYMin);
            float positionY = (BlockYMax + BlockYMin) / 2;
            // Compute
            var box = ComputeOBB(data2D);
            // Size Came From Alchemical
            Vector3 size = new Vector3(Vector3.Distance(box[0], box[3]), sizeY,
                Vector3.Distance(box[0], box[4]));
            Vector3 up = (box[0] - box[1]).normalized;
            Vector3 forward = (box[0] - box[4]).normalized;
            Quaternion rotation = Quaternion.LookRotation(forward, up);
            Vector3 center = new Vector3(box[8].x, positionY, box[8].z);
            return Matrix4x4.TRS(center, rotation, size);
        }
        private static Matrix4x4 ComputeGameObjectOBB_TrsMatrix2D(GameObject o)
        {
            var data3D = BoundUtility.GetGameObjectConvexHull(o);
            // Convert 2D
            BoundUtility.PositionData data2D = new BoundUtility.PositionData();
            List<Vector3> position2D = new List<Vector3>();
            for (int i = 0; i < data3D.Position.Length; i++)
            {
                position2D.Add(new Vector3(data3D.Position[i].x, 0, data3D.Position[i].z));
            }
            data2D.Position = position2D.ToArray();
            // Replace Box
            float BlockYMax = data3D.PositionY.Max();
            float BlockYMin = data3D.PositionY.Min();
            float sizeY = Math.Abs(BlockYMax - BlockYMin);
            float positionY = (BlockYMax + BlockYMin) / 2;
            // Compute
            var box = ComputeOBB(data2D);
            // Size Came From Alchemical
            Vector3 size = new Vector3(Vector3.Distance(box[0], box[3]), sizeY,
                Vector3.Distance(box[0], box[4]));
            Vector3 up = (box[0] - box[1]).normalized;
            Vector3 forward = (box[0] - box[4]).normalized;
            Quaternion rotation = Quaternion.LookRotation(forward, up);
            Vector3 center = new Vector3(box[8].x, positionY, box[8].z);
            return Matrix4x4.TRS(center, rotation, size);
        }
        private static Vector3[] ComputeOBB(BoundUtility.PositionData data)
        {
            // Prepare Data
            // Compute Convex Hull
            // TODO(QP) Use Smallest Bounding Sphere Center As Obb Center
            Vector3 obbCenter = AxisAlignedBoundingBox.GetAxisAlignedBoundingBoxTrsMatrix(data).GetT();
            // Compute Covariance Matrix With Outer Product
            Matrix4x4 cov = CalculateCovarianceMatrix(data.Position);
            int vertexCount = data.Position.Length;
            // Singular Value Decomposition
            ComputeShader cs =
                AssetDatabase.LoadAssetAtPath<ComputeShader>(
                    "Assets/CommonTools/CommonBounding/ComputeWithOBB.compute");// Editor
            int kernel = cs.FindKernel("ComputeSVD3D");

            GraphicsBuffer boundingBoxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, Marshal.SizeOf(typeof(BoundUtility.BoundingBoxBuffer)));
            GraphicsBuffer positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, Marshal.SizeOf(typeof(BoundUtility.PositionBuffer)));
            BoundUtility.BoundingBoxBuffer[] outputs = new BoundUtility.BoundingBoxBuffer[1];
            boundingBoxBuffer.SetData(outputs);
            positionBuffer.SetData(data.Position);
            // Dispatch Compute In CMD
            CommandBuffer cmd = new CommandBuffer();
            cmd.SetComputeVectorParam(cs, "_Center", obbCenter * vertexCount);
            cmd.SetComputeIntParam(cs, "_PointCount", vertexCount);
            cmd.SetComputeBufferParam(cs, kernel, "_BoundingBoxBuffer", boundingBoxBuffer);
            cmd.SetComputeBufferParam(cs, kernel, "_PositionBuffer", positionBuffer);
            // Split Matrix
            cmd.SetComputeVectorParam(cs, "_COV0", cov.GetColumn(0));
            cmd.SetComputeVectorParam(cs, "_COV1", cov.GetColumn(1));
            cmd.SetComputeVectorParam(cs, "_COV2", cov.GetColumn(2));
            cmd.SetComputeVectorParam(cs, "_COV3", cov.GetColumn(3));
            cmd.DispatchCompute(cs, kernel, 1, 1, 1);
            Graphics.ExecuteCommandBuffer(cmd);
            // Export
            boundingBoxBuffer.GetData(outputs);
            Vector3[] corner = new Vector3[9];
            corner[0] = outputs[0].P0;
            corner[1] = outputs[0].P1;
            corner[2] = outputs[0].P2;
            corner[3] = outputs[0].P3;
            corner[4] = outputs[0].P4;
            corner[5] = outputs[0].P5;
            corner[6] = outputs[0].P6;
            corner[7] = outputs[0].P7;
            // Center
            obbCenter = (corner[0] + corner[1] + corner[2] + corner[3] + corner[4] + corner[5] + corner[6] + corner[7]) / 8;
            corner[8] = obbCenter;
            
            boundingBoxBuffer.Release(); 
            positionBuffer.Release();
            cmd.Clear();
            return corner;
        }
        
        private static Matrix4x4 CalculateCovarianceMatrix(Vector3[] points)
        {
            Vector3 mean = Vector3.zero;
            foreach (Vector3 point in points)
            {
                mean += point;
            }
            mean /= points.Length;

            Matrix4x4 covarianceMatrix = new Matrix4x4();
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 diff = points[i] - mean;
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        covarianceMatrix[j, k] += diff[j] * diff[k];
                    }
                }
            }

            float scale = 1.0f / (points.Length - 1);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    covarianceMatrix[i, j] *= scale;
                }
            }

            return covarianceMatrix;
        }
        #endregion
    }
}