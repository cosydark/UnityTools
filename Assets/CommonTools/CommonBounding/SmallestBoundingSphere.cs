using UnityEditor;
using UnityEngine;

namespace CommonTools.Bounding
{
    public class SmallestBoundingSphere
    {
        public static Matrix4x4 GetSmallestSphere(Vector3[] vertices) => CustomStart(vertices);
        
        private static float radius;
        private static Vector3 center;
        private const float RadiusEpsilon = 1.00001f;
        private static Matrix4x4 trs;
        
        private static Matrix4x4 CustomStart(Vector3[] vertices)
        {
            BoundUtility.PositionData data = new BoundUtility.PositionData() { Position = vertices};
            CalculateWelzl(vertices, vertices.Length, 0, 0);
            return trs;
        }

        private static void CalculateWelzl(Vector3[] points, int length, int supportCount, int index)
        {
            switch (supportCount)
            {
                case 0:
                    radius = 0;
                    center = Vector3.zero;
                    break;
                case 1:
                    radius = 1.0f - RadiusEpsilon;
                    center = points[index - 1];
                    break;
                case 2:
                    SetSphere(points[index - 1], points[index - 2]);
                    break;
                case 3:
                    SetSphere(points[index - 1], points[index - 2], points[index - 3]);
                    break;
                case 4:
                    SetSphere(points[index - 1], points[index - 2], points[index - 3], points[index - 4]);
                    return;
            }

            for (int i = 0; i < length; i++)
            {
                Vector3 comp = points[i + index];
                float distSqr;

                distSqr = (comp - center).sqrMagnitude;

                if (distSqr - (radius * radius) > RadiusEpsilon - 1.0f)
                {
                    for (int j = i; j > 0; j--)
                    {
                        Vector3 a = points[j + index];
                        Vector3 b = points[j - 1 + index];
                        points[j + index] = b;
                        points[j - 1 + index] = a;
                    }
                    CalculateWelzl(points, i, supportCount + 1, index + 1);
                }
            }
        }

        private static void SetSphere(Vector3 O, Vector3 A)
        {
            radius = (float)System.Math.Sqrt(((A.x - O.x) * (A.x - O.x) + (A.y - O.y)
                * (A.y - O.y) + (A.z - O.z) * (A.z - O.z)) / 4.0f) + RadiusEpsilon - 1.0f;
            float x = (1 - .5f) * O.x + .5f * A.x;
            float y = (1 - .5f) * O.y + .5f * A.y;
            float z = (1 - .5f) * O.z + .5f * A.z;
            Vector3 sphereCenter = new Vector3(x, y, z);
            float sphereRadius = radius;
            trs = Matrix4x4.TRS(sphereCenter, Quaternion.identity, Vector3.one * sphereRadius * 2);
        }

        private static void SetSphere(Vector3 O, Vector3 A, Vector3 B)
        {
            Vector3 a = A - O;
            Vector3 b = B - O;
            Vector3 aCrossB = Vector3.Cross(a, b);
            float denom = 2.0f * Vector3.Dot(aCrossB, aCrossB);
            if (denom == 0)
            {
                center = Vector3.zero;
                radius = 0;
            }
            else
            {

                Vector3 o =
                    ((Vector3.Cross(aCrossB, a) * b.sqrMagnitude) + (Vector3.Cross(b, aCrossB) * a.sqrMagnitude)) /
                    denom;
                radius = o.magnitude * RadiusEpsilon;
                center = O + o;
            }
        }

        private static void SetSphere(Vector3 O, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 a = A - O;
            Vector3 b = B - O;
            Vector3 c = C - O;

            float denom = 2.0f * (a.x * (b.y * c.z - c.y * b.z) - b.x
                * (a.y * c.z - c.y * a.z) + c.x * (a.y * b.z - b.y * a.z));
            if (denom == 0)
            {
                center = Vector3.zero;
                radius = 0;
            }
            else
            {
                Vector3 o = ((Vector3.Cross(a, b) * c.sqrMagnitude)
                             + (Vector3.Cross(c, a) * b.sqrMagnitude)
                             + (Vector3.Cross(b, c) * a.sqrMagnitude)) / denom;
                radius = o.magnitude * RadiusEpsilon;
                center = O + o;
            }
        }
        
    }
}
