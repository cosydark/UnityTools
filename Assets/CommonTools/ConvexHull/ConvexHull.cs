using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ConvexHullFunctions
{
	public class ConvexHull
	{
		public static Vector3[] GenerateCorners(Vector3[] vertices)
		{
			var ch = ConvexHullCalculator.ConvexHull.Create(GetPoints(vertices));
			if (ch.Outcome == ConvexHullCalculator.ConvexHullCreationResultOutcome.Success)
			{
				HashSet<Vector3> chVertices = new HashSet<Vector3>();

				int t = 0;
				int v = 0;
				foreach (var f in ch.Result.Faces)
				{
					for (int fv = 0; fv < f.Vertices.Length; fv+=3)
					{
						var v1 = f.Vertices[fv];
						var v2 = f.Vertices[fv+1];
						var v3 = f.Vertices[fv+2];

						chVertices.Add(new Vector3((float)v1.Position[0],(float)v1.Position[1],(float)v1.Position[2]));
						chVertices.Add(new Vector3((float)v2.Position[0],(float)v2.Position[1],(float)v2.Position[2]));
						chVertices.Add(new Vector3((float)v3.Position[0],(float)v3.Position[1],(float)v3.Position[2]));
						t++;
						v+=3;
					}
				}
				return chVertices.ToArray();
			}
			return null;
		}
		public static Mesh Generate(Vector3[] vertices)
		{
			var ch = ConvexHullCalculator.ConvexHull.Create(GetPoints(vertices));
			if (ch.Outcome == ConvexHullCalculator.ConvexHullCreationResultOutcome.Success)
			{
				List<Vector3> chVertices = new List<Vector3>();
				List<Vector3> chNormals = new List<Vector3>();
				List<int> chTriangles = new List<int>();

				int t = 0;
				int v = 0;
				foreach (var f in ch.Result.Faces)
				{
					for (int fv = 0; fv < f.Vertices.Length; fv+=3)
					{
						var v1 = f.Vertices[fv];
						var v2 = f.Vertices[fv+1];
						var v3 = f.Vertices[fv+2];

						chVertices.Add(new Vector3((float)v1.Position[0],(float)v1.Position[1],(float)v1.Position[2]));
						chVertices.Add(new Vector3((float)v2.Position[0],(float)v2.Position[1],(float)v2.Position[2]));
						chVertices.Add(new Vector3((float)v3.Position[0],(float)v3.Position[1],(float)v3.Position[2]));
						chTriangles.Add(v);
						chTriangles.Add(v+1);
						chTriangles.Add(v+2);

						t++;
						v+=3;
					}
				}

				var mesh = new Mesh();
				mesh.SetVertices(chVertices);
				mesh.SetTriangles(chTriangles.ToArray(),0);
				mesh.RecalculateNormals();
				return mesh;
			} else
			{
				Debug.LogError("convex hull generation failed: " + ch.ErrorMessage);
			}

			return null;
		}

		static double[][] GetPoints(Vector3[] vertices)
		{
			var points = new double[vertices.Length][];
			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];
				points[i] = new double[]{v.x,v.y,v.z};
			}
			return points;
		}
	}

}
