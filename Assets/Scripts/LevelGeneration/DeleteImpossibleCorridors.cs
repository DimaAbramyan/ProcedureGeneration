using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;


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
        await DeleteUselessConnections();
    }
    public async UniTask DeleteUselessConnections()
    {
        foreach (RoomData fromRoom in floorData.rooms)
        {
            Dictionary<RoomData, CoridorData> correctCoridors = new Dictionary<RoomData, CoridorData>(fromRoom.coridors);
            foreach (RoomData toRoom in fromRoom.coridors.Keys)
            {
                if (!CanConnectCenters(toRoom, fromRoom))
                {
                    correctCoridors.Remove(toRoom);
                }
            }
            fromRoom.coridors = correctCoridors;
        }
        await UniTask.SwitchToMainThread();
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
    
}
public static class TileAlgorithm
{
    public static IEnumerable<Vector2Int> TilesOnLine(Vector2Int start, Vector2Int end, int counter = 1)
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
