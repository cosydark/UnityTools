using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CommonTools.Bounding
{
    public class OrientedBoundingBox
    {
        #region PrivateFunctions
        public static Vector3[] ComputeOBB(GameObject o)
        {
            // Prepare Data
            // Compute Convex Hull
            BoundUtility.PositionData data = BoundUtility.GetGameObjectConvexHull(o);
            Vector3 obbCenter = AxisAlignedBoundingBox.GetAxisAlignedBoundingCenter(data);
            // Compute Covariance Matrix With Outer Product
            Matrix4x4 cov = Matrix4x4.zero;
            int vertexCount = data.Position.Length;
            obbCenter /= vertexCount;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 p = data.Position[i] - obbCenter;
                cov = BoundMath.MatrixAddFloat(cov, BoundMath.OuterProduct(p, p));
            }
            cov = BoundMath.MatrixDivideFloat(cov, vertexCount);
            // Singular Value Decomposition
            ComputeShader cs =
                AssetDatabase.LoadAssetAtPath<ComputeShader>(
                    "Assets/CommonTools/CommonBounding/ComputeWithOBB.compute");// Editor
            int kernel = cs.FindKernel("ComputeSVD3D");

            GraphicsBuffer outputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, Marshal.SizeOf(typeof(BoundUtility.BoundingBoxBuffer)));
            GraphicsBuffer positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, Marshal.SizeOf(typeof(BoundUtility.PositionBuffer)));
            BoundUtility.BoundingBoxBuffer[] outputs = new BoundUtility.BoundingBoxBuffer[1];
            outputBuffer.SetData(outputs);
            positionBuffer.SetData(data.Position);
            
            CommandBuffer cmd = new CommandBuffer();
            cmd.SetComputeVectorParam(cs, "_Center", obbCenter);
            cmd.SetComputeIntParam(cs, "_PointCount", vertexCount);
            cmd.SetComputeBufferParam(cs, kernel, "_output_buffer", outputBuffer);
            cmd.SetComputeBufferParam(cs, kernel, "_position_buffer", positionBuffer);
            // Split Matrix
            cmd.SetComputeVectorParam(cs, "_COV0", cov.GetColumn(0));
            cmd.SetComputeVectorParam(cs, "_COV1", cov.GetColumn(1));
            cmd.SetComputeVectorParam(cs, "_COV2", cov.GetColumn(2));
            cmd.SetComputeVectorParam(cs, "_COV3", cov.GetColumn(3));
            cmd.DispatchCompute(cs, kernel, 1, 1, 1);
            Graphics.ExecuteCommandBuffer(cmd);
            // Export
            outputBuffer.GetData(outputs);
            Vector3[] corner = new Vector3[9];
            corner[0] = outputs[0].P0;
            corner[1] = outputs[0].P1;
            corner[2] = outputs[0].P2;
            corner[3] = outputs[0].P3;
            corner[4] = outputs[0].P4;
            corner[5] = outputs[0].P5;
            corner[6] = outputs[0].P6;
            corner[7] = outputs[0].P7;
            
            obbCenter = (corner[0] + corner[1] + corner[2] + corner[3] + corner[4] + corner[5] + corner[6] + corner[7]) / 8;
            corner[8] = obbCenter;

            outputBuffer.Release(); 
            positionBuffer.Release();
            cmd.Clear();
            return corner;
        }
        #endregion
    }
}