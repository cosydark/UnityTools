using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonTools.Bounding
{
    public class OrientedBoundingBox
    {
        #region PrivateFunctions
        private static Vector3[] ComputeOBBCorners(BoundUtility.PositionData data)
        {
            // Prepare Data
            Vector3[] axisAlignedBoundingBoxCorners = AxisAlignedBoundingBox.GetAxisAlignedBoundingCorners(data);
            Vector3 axisAlignedBoundingBoxCenter = axisAlignedBoundingBoxCorners[8];
            Vector3 obbCenter = axisAlignedBoundingBoxCenter;
            // Compute Covariance Matrix With Outer Product
            Matrix4x4 cov = Matrix4x4.zero;
            int vertexCount = data.Position.Length;
            obbCenter /= vertexCount;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 p = data.Position[i] - obbCenter;
                cov = BoundUtility.MatrixAddFloat(cov, BoundUtility.OuterProduct(p, p));
            }
            cov = BoundUtility.MatrixDivideFloat(cov, vertexCount);
            // Singular Value Decomposition
            
            return null;
        }
        #endregion
    }
}