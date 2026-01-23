using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;


public class ResolveBlockedEdges
{
    FloorData floorData;
    bool flippedRooms = true;
    Vector2 centersPerpend;
    int lenght = 150;
    private readonly FloorContext context;
    List<RoomData> checkedRooms;
    public ResolveBlockedEdges(FloorContext context)
    {
        this.context = context;
    }

    public async UniTask Run()
    {
        floorData = context.floorData;

        checkedRooms = new List<RoomData>();

        await ResolveImpossibleWays();
    }
    async UniTask ResolveImpossibleWays()
    {
        await DeleteUselessConnections();
        TryCreateCorridor(floorData);
    }
    void TryCreateCorridor(FloorData floorData)
    {
        List<Vector2Int> usefullWallsFrom = new List<Vector2Int>();
        List<Vector2Int> usefullWallsTo = new List<Vector2Int>();
        foreach (RoomData fromRoom in floorData.rooms)
        {
            foreach (RoomData toRoom in fromRoom.connectedRooms)
            {
                 (usefullWallsFrom, usefullWallsTo) = MinimizeWalls(fromRoom, toRoom);
            }
        }
    }
    public async UniTask DeleteUselessConnections()
    {
        var floorData = context.floorData;
        var roomPairs = new List<(RoomData from, RoomData to)>();
        foreach (var fromRoom in floorData.rooms)
        {
            foreach (var toRoom in fromRoom.connectedRooms)
            {
                roomPairs.Add((fromRoom, toRoom));
            }
        }

        var linksToKeep = new ConcurrentBag<(RoomData from, RoomData to)>();

        await UniTask.RunOnThreadPool(() =>
        {
            Parallel.ForEach(roomPairs, pair =>
            {
                if (CanConnectCenters(pair.from, pair.to, 4))
                {
                    linksToKeep.Add(pair);
                }
            });
        });

        // 3. Возвращаемся в главный поток для обновления connectedRooms
        await UniTask.SwitchToMainThread();

        foreach (var room in floorData.rooms)
        {
            var validConnections = linksToKeep
                .Where(x => x.from == room)
                .Select(x => x.to)
                .ToHashSet();

            room.connectedRooms = validConnections;
        }
    }

    bool CheckWithWidht(Vector2Int from, Vector2Int to, int widht)
    {
        for (int i = 0; i < widht - 1; i++)
        {

        }
        return false;
    }
    (List<Vector2Int>,List<Vector2Int>) MinimizeWalls(RoomData fromRoom, RoomData toRoom)
    {
        Vector2Int centerFrom = fromRoom.center;
        Vector2Int centerTo = toRoom.center;
        Vector2Int centersConnection = centerFrom - centerTo;

        centersPerpend = new Vector2(-centersConnection.y, centersConnection.x).normalized;
        float angle = Mathf.Atan2(centersConnection.y, centersConnection.x) * Mathf.Rad2Deg + 180;

        Debug.Log($"{fromRoom.number}, {toRoom.number}, угол между ними {angle}");
        return (GetUsefullWalls(fromRoom), GetUsefullWalls(toRoom));
    }
    List<Vector2Int> GetUsefullWalls(RoomData room)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        var roomSave = room.Walls.ToList();
        foreach (var wall in roomSave)
        {
            if (IsOnSide(wall.Key, room.center, centersPerpend, flippedRooms))
            {
                result.Add(wall.Key);
            }
        }
        flippedRooms = !flippedRooms;
        return result;
    }
    bool IsOnSide(Vector2 point, Vector2 linePoint, Vector2 lineDir, bool flipped)
    {
        Vector2 v = point - linePoint;
        float cross = lineDir.x * v.y - lineDir.y * v.x;

        return flipped ? cross >= 0f : cross < 0f;
    }
    bool CanConnectCenters(RoomData from, RoomData to, int maxHits)
    {
        int hits = 0;
        RoomData collidedRoom = null;
        foreach (var tilePos in TilesOnLine(from.center, to.center))
        {
            collidedRoom = context.floorData.GetRoomByTile(tilePos);
            if (!(collidedRoom == null) && (collidedRoom != from) && (collidedRoom != to))
                hits++;
            if (hits > maxHits)
            return false;
        }
        return true;
    }
    public static IEnumerable<Vector2Int> TilesOnLine(Vector2Int start, Vector2Int end)
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
}
