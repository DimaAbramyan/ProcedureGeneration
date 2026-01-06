using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangulationGenerator
{
    bool triangulationFinished = false;
    FloorData floorData;
    List<Triangle> triangles;
    Triangle superstructure;
    List<Vector2Int> unsortedPoints;
    public Material lineMaterial;
    List<LineRenderer> lines = new List<LineRenderer>();
    [SerializeField] MinOstTreeGenerator minOstTree;
    [SerializeField] private LevelGenerator lvlGenerator;

    private readonly FloorContext context;
    public TriangulationGenerator(FloorContext context)
    {
        this.context = context;
    }

    public void Run()
    {
        floorData = context.floorData;
        Triangulation(floorData);
    }
    public void CreateSuperstructure()
    {
        Vector2Int minXY = floorData.GetFloorMinXY() - new Vector2Int(1, 1);
        Vector2Int maxXY = floorData.GetFloorMaxXY() + new Vector2Int(1, 1);
        superstructure = new Triangle(minXY,
            new Vector2Int(minXY.x, maxXY.y * 2),
            new Vector2Int(maxXY.x * 2, minXY.y));
    }
    public void Triangulation(FloorData floorData)
    {
        CreateSuperstructure();
        triangles = new List<Triangle>
        {
            superstructure
        };
        unsortedPoints = new List<Vector2Int>();
        foreach (RoomData room in floorData.rooms)
        {
            unsortedPoints.Add(room.center);
        }
        while (unsortedPoints.Count > 0)
        {
            Vector2Int point = unsortedPoints[0];
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
                Vector2Int[] v = t.GetPoints();
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
                triangles.Add(new Triangle(e.a,e.b, point));
            }
        }
        RemoveSuperstructureTriangles();
        triangulationFinished = true;
        DrawTriangles(triangles);
        foreach(var triangle in triangles)
{
            Vector2Int[] points = triangle.GetPoints();
            RoomData[] RoomsToConnect = new RoomData[3];

            for (int i = 0; i < points.Length; i++)
            {
                RoomsToConnect[i] = floorData.GetRoomDataByCenter(points[i]);
            }

            for (int i = 0; i < RoomsToConnect.Length; i++)
            {
                if (RoomsToConnect[i] == null)
                    Debug.LogWarning($"Комната с центром {points[i]} не найдена!");
                for (int j = i + 1; j < RoomsToConnect.Length; j++)
                {
                    RoomsToConnect[i].AddConectedRoom(RoomsToConnect[j]);
                    RoomsToConnect[j].AddConectedRoom(RoomsToConnect[i]);
                }
            }
        }
        foreach (var roomData in context.floorData.rooms)
        {
            Debug.Log($"Номер комнаты: {roomData.number},его центр: {roomData.center}, число соседей: {roomData.connectedRooms.Count}");
        }

        GenerationTimer.Watch.Stop();
        Debug.Log(
            $"Generation time: {GenerationTimer.Watch.ElapsedMilliseconds} ms"
        );
    }
    private void RemoveSuperstructureTriangles()
    {
        if (superstructure == null || triangles == null)
            return;

        Vector2Int[] s = superstructure.GetPoints();
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
            Vector3 down = new Vector3(0, 1, 0);
            CreateLine(ToVector3XZ(p[0]) - down, ToVector3XZ(p[1]) - down);
            CreateLine(ToVector3XZ(p[1]) - down, ToVector3XZ(p[2]) - down);
            CreateLine(ToVector3XZ(p[2]) - down, ToVector3XZ(p[0]) - down);


            CreateLine((Vector2)p[0], (Vector2)p[1]);
            CreateLine((Vector2)p[1], (Vector2)p[2]);
            CreateLine((Vector2)p[2], (Vector2)p[0]);
        }
    }

    void CreateLine(Vector3 a, Vector3 b)
    {
        var go = new GameObject("Line");

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
            Object.Destroy(l.gameObject);
        lines.Clear();
    }
    Vector3 ToVector3XZ(Vector2 v)
    {
        return new Vector3(v.x, 0f, v.y);
    }

}
