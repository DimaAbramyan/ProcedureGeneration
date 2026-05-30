using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinOstTreeGenerator
{
    private readonly FloorContext context;
    private List<CoridorData> allCoridors;

    public MinOstTreeGenerator(FloorContext context)
    {
        this.context = context;
    }

    public void Run()
    {
        allCoridors = context.GetAllCoridors();
        
        if (allCoridors == null || allCoridors.Count == 0)
        {
            Debug.LogError("No corridors found!");
            return;
        }

        FindMinOstTree(context.floorData);
        
    }

    private void FindMinOstTree(FloorData floorData)
    {
        uint seed = context.seed;

        Dictionary<RoomData, int> roomIds = new();

        for (int i = 0; i < floorData.RoomByID.Values.Count; i++)
        {
            roomIds[floorData.RoomByID[i]] = i;
        }

        List<CoridorData> validCoridors = allCoridors
            .Where(c =>
                c != null &&
                c.FromRoom != null &&
                c.ToRoom != null &&
                c.FromRoom != c.ToRoom &&
                roomIds.ContainsKey(c.FromRoom) &&
                roomIds.ContainsKey(c.ToRoom))
            .ToList();

        validCoridors = validCoridors
            .OrderBy(c =>
                Vector2Int.Distance(
                    c.FromRoom.center,
                    c.ToRoom.center))
            .ToList();

        DisjointSet ds = new(floorData.RoomByID.Values.Count);

        List<CoridorData> mst = new();

        foreach (var c in validCoridors)
        {
            Debug.LogWarning($"[MST EDGE] From {c.FromRoom.id} hash {c.FromRoom.GetHashCode()}");
            Debug.Log($"[MST EDGE] To   {c.ToRoom.id} hash {c.ToRoom.GetHashCode()}");

            int a = roomIds[c.FromRoom];
            int b = roomIds[c.ToRoom];

            if (ds.Find(a) == ds.Find(b))
                continue;

            ds.Union(a, b);
            mst.Add(c);
        }

        HashSet<int> roots = new();

        for (int i = 0; i < floorData.RoomByID.Values.Count; i++)
        {
            roots.Add(ds.Find(i));
        }

        Debug.Log(
            $"MST components: {roots.Count}, " +
            $"RoomByID.Values: {floorData.RoomByID.Values.Count}, " +
            $"mst edges: {mst.Count}");

        // если > 1 компоненты —
        // значит граф изначально несвязный
        if (roots.Count > 1)
        {
            Debug.LogError(
                "CORRIDOR GRAPH IS DISCONNECTED BEFORE MST!");
        }

        List<CoridorData> extra = new();

        foreach (var c in validCoridors)
        {
            if (mst.Contains(c))
                continue;

            float r = GetNoise(
                c.FromRoom.center,
                c.ToRoom.center,
                seed);

            if (r < context.coridorPercent)
            {
                extra.Add(c);
            }
        }

        List<CoridorData> finalCoridors = new();

        finalCoridors.AddRange(mst);
        finalCoridors.AddRange(extra);
        BuildCorridors(floorData, finalCoridors);

        CheckConnectivity(floorData, finalCoridors);
    }

    private void BuildCorridors(
        FloorData floorData,
        List<CoridorData> coridors)
    {
        foreach (var room in floorData.RoomByID.Values)
        {
            room.floor.coridors.Clear();
        }

        foreach (var c in coridors)
        {
            if (c == null)
                continue;

            RoomData a = c.FromRoom;
            RoomData b = c.ToRoom;

            Debug.LogWarning($"[BUILD] FROM graph hash {a.GetHashCode()} ({a.id})");
            Debug.LogWarning($"[BUILD] TO   graph hash {b.GetHashCode()} ({b.id})");

            if (a == null || b == null)
                continue;

            if (!a.floor.coridors.ContainsKey((a.id, b.id)))
            {
                a.floor.coridors.Add((a.id, b.id), c);
            }

            if (!b.floor.coridors.ContainsKey((a.id, b.id)))
            {
                b.floor.coridors.Add((a.id, b.id), c);
            }
        }
    }

    private void CheckConnectivity(
        FloorData floorData,
        List<CoridorData> coridors)
    {
        if (floorData.RoomByID.Values.Count == 0)
            return;

        HashSet<int> visited = new();
        Queue<int> queue = new();

        int start = floorData.RoomByID[0].id;

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            int cur = queue.Dequeue();

            foreach (var next in floorData.RoomByID.Values)
            {
                if (visited.Contains(next.id))
                    continue;

                visited.Add(next.id);
                queue.Enqueue(next.id);
            }
        }

        Debug.LogError(
            $"After MST - Connected: " +
            $"{visited.Count}/{floorData.RoomByID.Values.Count}, " +
            $"Final corridors: {coridors.Count}");

        if (visited.Count != floorData.RoomByID.Values.Count)
        {
            foreach (var room in floorData.RoomByID.Values)
            {
                if (!visited.Contains(room.id))
                {
                    Debug.LogError(
                        $"DISCONNECTED ROOM: {room.id}");
                }
            }
        }
    }

    private float GetNoise(
        Vector2Int a,
        Vector2Int b,
        uint seed)
    {
        int h = Hash(a) ^ Hash(b) ^ (int)seed;

        unchecked
        {
            h = (h << 13) ^ h;

            int result =
                (h * (h * h * 15731 + 789221)
                + 1376312589);

            return (result & 0x7fffffff)
                / (float)int.MaxValue;
        }
    }

    private int Hash(Vector2Int v)
    {
        unchecked
        {
            return v.x * 73856093
                 ^ v.y * 19349663;
        }
    }
}