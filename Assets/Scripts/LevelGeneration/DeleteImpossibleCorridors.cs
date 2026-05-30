using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor;


public class DeleteImpossibleCorridors
{
    FloorData floorData;
    private readonly FloorContext context;
    List<RoomData> checkedRooms;
    public DeleteImpossibleCorridors(FloorContext context)
    {
        this.context = context;
    }

    public async UniTask Run()
    {
        floorData = context.floorData;

        await ResolveImpossibleWays();
    }
    async UniTask ResolveImpossibleWays()
    {
        await DeleteUselessConnectionsBetweenRooms();
        await DeleteUselessConnectionsByCorridors();
    }
    public async UniTask DeleteUselessConnectionsBetweenRooms()
    {
        var keys = floorData.coridors.Keys.ToList();

        foreach (var (aId, bId) in keys)
        {
            if (!floorData.RoomByID.TryGetValue(aId, out var fromRoom))
                continue;

            if (!floorData.RoomByID.TryGetValue(bId, out var toRoom))
                continue;

            if (!floorData.coridors.TryGetValue((aId, bId), out var corridor))
                continue;

            if (!CanConnectCenters(fromRoom, toRoom))
            {
                RemoveCorridor(corridor);
            }
        }

        await UniTask.Yield();
    }
    public async UniTask DeleteUselessConnectionsByCorridors()
    {
        uint seed = context.seed;

        List<CoridorData> allCoridors = new();

        foreach (var room in floorData.RoomByID.Values)
        {
            foreach (var c in room.floor.coridors.Values)
            {
                if (c == null)
                    continue;

                if (c.FromRoom == null || c.ToRoom == null)
                    continue;

                if (!allCoridors.Contains(c))
                    allCoridors.Add(c);
            }
        }
        allCoridors = allCoridors
        .Where(c =>
        c != null &&
        c.FromRoom != null &&
        c.ToRoom != null &&
        c.Tiles != null)
        .ToList();
        allCoridors = allCoridors
            .OrderByDescending(c => GetPriority(c, seed))
            .ToList();

        HashSet<CoridorData> removed = new();

        for (int i = 0; i < allCoridors.Count; i++)
        {
            var a = allCoridors[i];

            if (removed.Contains(a))
                continue;

            for (int j = i + 1; j < allCoridors.Count; j++)
            {
                var b = allCoridors[j];

                if (removed.Contains(b))
                    continue;

                if (a.FromRoom == b.FromRoom ||
                    a.FromRoom == b.ToRoom ||
                    a.ToRoom == b.FromRoom ||
                    a.ToRoom == b.ToRoom)
                    continue;

                if (!Intersects(a, b))
                    continue;

                float pa = GetPriority(a, seed);
                float pb = GetPriority(b, seed);

                if (pa >= pb)
                {
                    RemoveCorridor(b);
                    removed.Add(b);
                }
                else
                {
                    RemoveCorridor(a);
                    removed.Add(a);
                    break;
                }
            }
        }

        await UniTask.Yield();
    }
    bool CanConnectCenters(RoomData from, RoomData to, int maxHits = 0)
    {
        int hits = 0;
        RoomData collidedRoom = null;
        foreach (var tilePos in TileAlgorithm.TilesOnLine(from.center, to.center))
        {
            collidedRoom = context.floorData.GetRoomByTile(tilePos);
            if (!(collidedRoom == null) && (collidedRoom != from) && (collidedRoom != to))
                hits++;
            if (hits > maxHits)
                return false;
        }
        return true;
    }
    float GetPriority(CoridorData c, uint seed)
    {
        float dist = Vector2Int.Distance(c.FromRoom.center, c.ToRoom.center);

        float noise = GetNoise(c.FromRoom.center, c.ToRoom.center, seed);

        return -dist * 2f + noise * 5f;
    }
    bool Intersects(CoridorData a, CoridorData b)
    {
        if (a == null || b == null)
            return false;

        if (a.Tiles == null || b.Tiles == null)
            return false;

        foreach (var t in a.Tiles)
        {
            if (b.Tiles.Contains(t))
                return true;
        }

        return false;
    }
    void RemoveCorridor(CoridorData c)
    {
        if (c == null)
            return;

        if (c.FromRoom != null && c.ToRoom != null)
        {
            floorData.coridors.Remove((c.ToRoom.id, c.FromRoom.id));
        }
    }
    private float GetNoise(Vector2Int a, Vector2Int b, uint seed)
    {
        int h = Hash(a) ^ Hash(b) ^ (int)seed;

        unchecked
        {
            h = (h << 13) ^ h;
            int result = (h * (h * h * 15731 + 789221) + 1376312589);

            return (result & 0x7fffffff) / (float)int.MaxValue;
        }
    }
    private int Hash(Vector2Int v)
    {
        unchecked
        {
            return v.x * 73856093 ^ v.y * 19349663;
        }
    }
}
public static class TileAlgorithm
{
    public static IEnumerable<Vector2Int> TilesOnLine(Vector2Int start, 
        Vector2Int end)
    {
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = err * 2;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }

            yield return new Vector2Int(x0, y0);
        }
    }
    public static IEnumerable<Vector2Int> TilesOnLineSmooth(
    Vector2Int start,
    Vector2Int end)
    {
        int x = start.x;
        int y = start.y;

        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;

        int err = dx - dy;

        Vector2Int last = new Vector2Int(x, y);

        while (true)
        {
            yield return new Vector2Int(x, y);

            if (x == end.x && y == end.y)
                break;

            int e2 = err * 2;

            bool stepX = e2 > -dy;
            bool stepY = e2 < dx;

            if (stepX && stepY)
            {
                if (dx > dy)
                    stepY = false;
                else
                    stepX = false;
            }

            if (stepX)
            {
                err -= dy;
                x += sx;
            }
            else if (stepY)
            {
                err += dx;
                y += sy;
            }
        }
    }
}
