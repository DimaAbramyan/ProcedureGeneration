using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangulationGenerator
{
    HashSet<(RoomData, RoomData)> edges;

    GameObject TriangulationHandler;
    bool DrawLines = false;
    bool triangulationFinished = false;
    FloorData floorData;
    List<Triangle> triangles;
    Triangle superstructure;
    List<Vector2Int> unsortedPoints;
    public Material lineMaterial;
    List<LineRenderer> lines = new List<LineRenderer>();
    [SerializeField] MinOstTreeGenerator minOstTree;
    [SerializeField] private LevelBuilder lvlGenerator;

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
        foreach (RoomData room in floorData.RoomByID.Values)
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
                    if ((e.A == e2.A && e.B == e2.B) || (e.A == e2.B && e.B == e2.A))
                        count++;
                }
                if (count == 1)
                    boundary.Add(e);
            }

            foreach (var t in badTriangles)
                triangles.Remove(t);

            foreach (var e in boundary)
            {
                triangles.Add(new Triangle(e.A,e.B, point));
            }
        }
        RemoveSuperstructureTriangles();
        triangulationFinished = true;
        context.triangles = triangles;
        foreach (var triangle in triangles)
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
                    RoomsToConnect[i].AddConectedRoom(floorData, RoomsToConnect[j]);
                }
            }
        }
        foreach (var roomData in context.floorData.RoomByID.Values)
        {
           // Debug.Log($"Номер комнаты: {roomData.number},его центр: {roomData.center}, число соседей: {roomData.connectedRooms.Count}");
        }

        //GenerationTimer.Watch.Stop();
        //Debug.Log(
        //    $"Generation time: {GenerationTimer.Watch.ElapsedMilliseconds} ms"
        //);
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
