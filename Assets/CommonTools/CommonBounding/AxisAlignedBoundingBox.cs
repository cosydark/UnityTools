using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace CommonTools.Bounding
{
    public abstract class AxisAlignedBoundingBox
    {
        // Public Functions
        public static Matrix4x4 GetAxisAlignedArtBlock(GameObject gameObject) => ComputeGameObjectAABB_ArtBlock(gameObject);
        public static Vector3[] GetAxisAlignedBoundingBox(GameObject gameObject) => ComputeGameObjectAABB_Box(gameObject);
        public static Vector3 GetAxisAlignedBoundingCenter(GameObject gameObject) => ComputeGameObjectAABB_Center(gameObject);
        public static Vector3 GetAxisAlignedBoundingCenter(BoundUtility.PositionData data) => ComputeGameObjectAABB_Center(data);
        public static Vector3 GetAxisAlignedBoundingCenterBot(GameObject gameObject) => ComputeGameObjectAABB_CenterBot(gameObject);
        public static Vector3 GetAxisAlignedBoundingSize(GameObject gameObject) => ComputeGameObjectAABB_Size(gameObject);
        public static Vector3[] GetAxisAlignedBoundingCorners(GameObject gameObject) => ComputeGameObjectAABB_Corners(gameObject);
        public static Vector3[] GetAxisAlignedBoundingCorners(BoundUtility.PositionData data) => ComputeAABB(data);
        
        #region PrivateFunctions
        
        private static Matrix4x4 ComputeGameObjectAABB_ArtBlock(GameObject o)
        {
            var data = BoundUtility.GetGameObjectConvexHull(o);
            Vector3[] corners = ComputeAABB(data);
            Vector3 center = corners[8];
            Vector3 size = corners[10];
            return Matrix4x4.TRS(center, quaternion.identity, size);
        }
        //     Forward Direction
        //     (1, 5)           (2, 6)
        //      ________________
        //     |                |
        //     |                |
        //     |    Top View    |     Right Direction
        //     |                |
        //     |                |
        //      ________________
        //     (0, 4)           (3, 7)
        private static Vector3[] ComputeGameObjectAABB_Box(GameObject o)
        {
            var data = BoundUtility.GetGameObjectConvexHull(o);
            Vector3[] corners = ComputeAABB(data);
            List<Vector3> box = new List<Vector3>();
            for (int i = 0; i < 8; i++)
            {
                box.Add(corners[i]);
            }
            return box.ToArray();
        }

        private static Vector3 ComputeGameObjectAABB_Center(GameObject o)
        {
            var data = BoundUtility.GetGameObjectConvexHull(o);
            return ComputeAABB(data)[8];
        }
        
        private static Vector3 ComputeGameObjectAABB_Center(BoundUtility.PositionData data)
        {
            return ComputeAABB(data)[8];
        }
        
        private static Vector3 ComputeGameObjectAABB_CenterBot(GameObject o)
        {
            var data = BoundUtility.GetGameObjectConvexHull(o);
            return ComputeAABB(data)[9];
        }
        
        private static Vector3 ComputeGameObjectAABB_Size(GameObject o)
        {
            var data = BoundUtility.GetGameObjectConvexHull(o);
            return ComputeAABB(data)[10];
        }
        
        private static Vector3[] ComputeGameObjectAABB_Corners(GameObject o)
        {
            var data = BoundUtility.GetGameObjectConvexHull(o);
            return ComputeAABB(data);
        }
        /// <summary>
        /// 0 - 7 => Eight Corner, 8 => Center, 9 => Bottom Center, 10 => Size
        /// </summary>
        private static Vector3[] ComputeAABB(BoundUtility.PositionData data)
        {
            Vector3 min = new Vector3(data.PositionX.Min(), data.PositionY.Min(), data.PositionZ.Min());
            Vector3 max = new Vector3(data.PositionX.Max(), data.PositionY.Max(), data.PositionZ.Max());
        
            Vector3[] corner = new Vector3[11];
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
        #endregion
    }
}

