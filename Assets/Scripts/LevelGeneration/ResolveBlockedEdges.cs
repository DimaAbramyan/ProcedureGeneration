using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class ResolveBlockedEdges
{
    List<Vector2Int> usefulWallsFrom;
    List<Vector2Int> usefulWallsTo;
    bool flippedRooms = true;
    Vector2 centersPerpend;
    int lenght = 150;
    private readonly FloorContext context;
    List<RoomData> checkedRooms;
    public ResolveBlockedEdges(FloorContext context)
    {
        this.context = context;
    }

    public void Run()
    {
        FloorData floorData = context.floorData;

        checkedRooms = new List<RoomData>();

        ResolveImpossibleWays(floorData);
    }
    void ResolveImpossibleWays(FloorData floorData)
    {
        //MinimizeWalls(floorData.rooms[0], floorData.rooms[2]);
        //MinimizeWalls(floorData.rooms[10], floorData.rooms[6]);
        //MinimizeWalls(floorData.rooms[7], floorData.rooms[3]);
        //MinimizeWalls(floorData.rooms[1], floorData.rooms[8]);
        //MinimizeWalls(floorData.rooms[4], floorData.rooms[12]);
        //MinimizeWalls(floorData.rooms[11], floorData.rooms[5]);
        //Debug.Log("<---------------------------------------------->");
        //foreach (var wall in floorData.rooms[9].Walls.Keys)
        //{
        //    Debug.Log(wall);
        //}
        //Debug.Log("<---------------------------------------------->");
        foreach (RoomData fromRoom in floorData.rooms)
        {
            foreach (RoomData toRoom in fromRoom.connectedRooms)
            {
                if (checkedRooms.Contains(fromRoom) || (!CanConnectCenters(fromRoom, toRoom, 2)))
                {
                    continue;
                }
                (usefulWallsFrom, usefulWallsTo) = MinimizeWalls(fromRoom, toRoom);

            }
        }
    }
    void TryCreateCorridor(List<Vector2Int> fromWalls, List<Vector2Int> toWalls, RoomData fromRoom, RoomData toRoom)
    {
        bool possibleToConnect = true;
        RoomData collidedRoom = null;
        foreach (Vector2Int from in fromWalls)
        {
            foreach (Vector2Int to in toWalls)
            {
                possibleToConnect = true;
                foreach (var tile in TilesOnLine(from, to))// Проверка на то, что линия не пересекает другие комнаты
                {
                    collidedRoom = context.floorData.WhichRoomTile(tile);
                    if (!(collidedRoom == null || collidedRoom == fromRoom))
                    {
                        possibleToConnect = false;
                        break;
                    }
                }
                if (possibleToConnect)
                {
                    CheckWithWidht(from, to, 2);
                    return;
                }
            }
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
            collidedRoom = context.floorData.WhichRoomTile(tilePos);
            if (!(collidedRoom == null || collidedRoom == from || collidedRoom == to))  
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

   
    
    void CreateLine(Vector3Int a, Vector3Int b)
    {
        var go = new GameObject("DifferentLine");
        var lr = go.AddComponent<LineRenderer>();
        lr.material = null;
        lr.positionCount = 2;
        lr.startWidth = 1f;
        lr.endWidth = 1f;
        lr.useWorldSpace = true;
        lr.SetPosition(0, (Vector3)a);
        lr.SetPosition(1, (Vector3)b);
    }
    void CreateLine(Vector2Int a, Vector2Int b)
    {
        var go = new GameObject("DifferentLine");
        var lr = go.AddComponent<LineRenderer>();
        lr.material = null;
        lr.positionCount = 2;
        lr.startWidth = 1f;
        lr.endWidth = 1f;
        lr.useWorldSpace = true;
        lr.SetPosition(0, (Vector2)a);
        lr.SetPosition(1, (Vector2)b);
    }

    void CreateLine(Vector3Int a, Vector2Int b)
    {
        var go = new GameObject("DifferentLine");
        var lr = go.AddComponent<LineRenderer>();
        lr.material = null;
        lr.positionCount = 2;
        lr.startWidth = 1f;
        lr.endWidth = 1f;
        lr.useWorldSpace = true;
        lr.SetPosition(0, (Vector3)a);
        lr.SetPosition(1, (Vector2)b);
    }

    void CreateLine(Vector2Int a, Vector3Int b)
    {
        var go = new GameObject("DifferentLine");
        var lr = go.AddComponent<LineRenderer>();
        lr.material = null;
        lr.positionCount = 2;
        lr.startWidth = 1f;
        lr.endWidth = 1f;
        lr.useWorldSpace = true;
        lr.SetPosition(0, (Vector2)a);
        lr.SetPosition(1, (Vector3)b);
    }
}
