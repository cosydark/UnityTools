using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonTools.Bounding;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class BoudingBoxDrawer : MonoBehaviour
{
    public DrawType Type = DrawType.Line;
    
    private List<Vector3> startPos = new List<Vector3>();
    private List<Vector3> endPos = new List<Vector3>();
    private List<Vector3> point = new List<Vector3>();

    public enum DrawType
    {
        Line,
        Point
    }
    
    [Button("Set Up AABB Lines", ButtonSizes.Medium)]
    private void SetupAABBLines()
    {
        startPos = new List<Vector3>();
        endPos = new List<Vector3>();
        
        Vector3[] box = CommonTools.Bounding.AxisAlignedBoundingBox.GetAxisAlignedBoundingCorners(this.gameObject);
        // Z
        startPos.Add(box[0]); endPos.Add(box[1]);
        startPos.Add(box[2]); endPos.Add(box[3]);
        startPos.Add(box[4]); endPos.Add(box[5]);
        startPos.Add(box[6]); endPos.Add(box[7]);
        // X
        startPos.Add(box[0]); endPos.Add(box[3]);
        startPos.Add(box[1]); endPos.Add(box[2]);
        startPos.Add(box[4]); endPos.Add(box[7]);
        startPos.Add(box[5]); endPos.Add(box[6]);
        // Y
        startPos.Add(box[0]); endPos.Add(box[4]);
        startPos.Add(box[1]); endPos.Add(box[5]);
        startPos.Add(box[2]); endPos.Add(box[6]);
        startPos.Add(box[3]); endPos.Add(box[7]);
    }
    
    [Button("Set Up AABB ArtBlock", ButtonSizes.Medium)]
    private void SetupAABBArtBlock()
    {
        GameObject artBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        artBlock.name = $"{this.gameObject.name} AABB";
        Matrix4x4 trs = CommonTools.Bounding.AxisAlignedBoundingBox.GetAxisAlignedArtBlock(this.gameObject);
        // Set Transform
        if (!trs.ValidTRS()) { return; }
        artBlock.transform.position = trs.GetT();
        artBlock.transform.rotation = trs.GetR();
        artBlock.transform.localScale = trs.GetS();
    }
    
   [Button("Set Up Convex Hull Lines", ButtonSizes.Medium)]
   private void SetupConvexHullLines()
   {
       startPos = new List<Vector3>();
       endPos = new List<Vector3>();
        
       Vector3[] box = CommonTools.Bounding.OrientedBoundingBox.ComputeOBB(this.gameObject);
       // Z
       startPos.Add(box[0]); endPos.Add(box[1]);
       startPos.Add(box[2]); endPos.Add(box[3]);
       startPos.Add(box[4]); endPos.Add(box[5]);
       startPos.Add(box[6]); endPos.Add(box[7]);
       // X
       startPos.Add(box[0]); endPos.Add(box[3]);
       startPos.Add(box[1]); endPos.Add(box[2]);
       startPos.Add(box[4]); endPos.Add(box[7]);
       startPos.Add(box[5]); endPos.Add(box[6]);
       // Y
       startPos.Add(box[0]); endPos.Add(box[4]);
       startPos.Add(box[1]); endPos.Add(box[5]);
       startPos.Add(box[2]); endPos.Add(box[6]);
       startPos.Add(box[3]); endPos.Add(box[7]);
   }
   
    private void Update()
    {
        if (Type == DrawType.Line)
        {
            for (int i = 0; i < startPos.Count; i++)
            {
                Debug.DrawLine(startPos[i], endPos[i], Color.green);
            }
        }
        
        if (Type == DrawType.Point)
        {
            for (int i = 0; i < point.Count; i++)
            {
                Debug.DrawLine(point[i], point[i] + Vector3.up * 0.05f, Color.green);
            }
        }
    }

}
