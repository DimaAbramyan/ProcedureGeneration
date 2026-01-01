using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangulationDelone : MonoBehaviour
{
    bool triangulationFinished = false;

    FloorData floorData;
    List<Triangle> triangles;
    Triangle superstructure;
    List<Vector2> unsortedPoints;
    public Material lineMaterial;
    List<LineRenderer> lines = new List<LineRenderer>();
    [SerializeField] MinOstTree minOstTree;
    public void Init(FloorData data)
    {
        floorData = data;
    }
    public void CreateSuperstructure()
    {
        Vector2Int minXY = floorData.GetFloorMinXY() - new Vector2Int(1, 1);
        Vector2Int maxXY = floorData.GetFloorMaxXY() + new Vector2Int(1, 1);
        superstructure = new Triangle(minXY,
            new Vector2Int(minXY.x, maxXY.y * 2),
            new Vector2Int(maxXY.x * 2, minXY.y));
    }
    public void Triangulation()
    {
        CreateSuperstructure();
        triangles = new List<Triangle>
        {
            superstructure
        };
        unsortedPoints = new List<Vector2>();
        foreach (RoomData room in floorData.rooms)
        {
            unsortedPoints.Add(room.center);
        }
        while (unsortedPoints.Count > 0)
        {
            Vector2 point = unsortedPoints[0];
            unsortedPoints.RemoveAt(0);

            List<Triangle> badTriangles = new List<Triangle>();
            foreach (var t in triangles)
            {
                if (t.CheckIfPointInTriangle(point))
                    badTriangles.Add(t);
            }

            List<Edge> edges = new List<Edge>();
            foreach (var t in badTriangles)
            {
                Vector2[] v = t.GetPoints();
                edges.Add(new Edge(v[0], v[1]));
                edges.Add(new Edge(v[1], v[2]));
                edges.Add(new Edge(v[2], v[0]));
            }

            List<Edge> boundary = new List<Edge>();
            foreach (var e in edges)
            {
                int count = 0;
                foreach (var e2 in edges)
                {
                    if ((e.a == e2.a && e.b == e2.b) || (e.a == e2.b && e.b == e2.a))
                        count++;
                }
                if (count == 1)
                    boundary.Add(e);
            }

            foreach (var t in badTriangles)
                triangles.Remove(t);

            foreach (var e in boundary)
            {
                triangles.Add(new Triangle(e.a, e.b, point));
            }
        }
        RemoveSuperstructureTriangles();
        triangulationFinished = true;
        DrawTriangles(triangles);
        minOstTree.SetValue(triangles);
        minOstTree.Init();
    }
    private void RemoveSuperstructureTriangles()
    {
        if (superstructure == null || triangles == null)
            return;

        Vector2[] s = superstructure.GetPoints();
        Vector2 A = s[0];
        Vector2 B = s[1];
        Vector2 C = s[2];

        triangles.RemoveAll(t =>
        {
            var p = t.GetPoints();
            return p[0] == A || p[0] == B || p[0] == C ||
                   p[1] == A || p[1] == B || p[1] == C ||
                   p[2] == A || p[2] == B || p[2] == C;
        });
    }
    
    public void DrawTriangles(List<Triangle> triangles)
    {
        Clear();

        foreach (var t in triangles)
        {
            var p = t.GetPoints();

            CreateLine(p[0], p[1]);
            CreateLine(p[1], p[2]);
            CreateLine(p[2], p[0]);
        }
    }

    void CreateLine(Vector3 a, Vector3 b)
    {
        var go = new GameObject("Line");
        go.transform.parent = transform;

        var lr = go.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.startWidth = 1f;
        lr.endWidth = 1f;
        lr.useWorldSpace = true;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        lines.Add(lr);
    }

    void Clear()
    {
        foreach (var l in lines)
            Destroy(l.gameObject);
        lines.Clear();
    }

}
