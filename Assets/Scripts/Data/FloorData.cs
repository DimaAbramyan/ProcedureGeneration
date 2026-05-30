using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Unity.VisualScripting;
using UnityEngine;
public class FloorData
{
    private Dictionary<int, List<CellSpan>> rows = new();
    public Dictionary<int, RoomData> RoomByID;
    public Dictionary<(int a, int b), CoridorData> coridors;
    Vector2Int FloorMaxXY;
    Vector2Int FloorMinXY;

    public int FloorCount;
    public FloorData()
    {
        coridors = new Dictionary<(int a, int b), CoridorData>();
        RoomByID = new Dictionary<int, RoomData>();
        rows = new Dictionary<int, List<CellSpan>>();

        FloorMaxXY = new Vector2Int(int.MinValue, int.MinValue);
        FloorMinXY = new Vector2Int(int.MaxValue, int.MaxValue);
    }
    public void AddMapRoom(Vector2Int from, Vector2Int to, RoomData room)
    {
        for (int y = from.y; y <= to.y; y++)
        {
            if (!rows.TryGetValue(y, out var spans))
            {
                spans = new List<CellSpan>();
                rows[y] = spans;
            }

            spans.Add(new CellSpan
            {
                xMin = (ushort)from.x,
                xMax = (ushort)to.x,
                room = room
            });
        }
    }
    public RoomData GetRoomByTile(Vector2Int coord)
    {
        if (!rows.TryGetValue(coord.y, out var spans))
            return null;

        foreach (var span in spans)
        {
            if (span.Contains(coord.x))
                return span.room;
        }

        return null;
    }
    public void AddRoom(RoomData room)
    {
        RoomByID.Add(room.id, room);
        CheckMinMaxXY(room.MinTileXY, room.MaxTileXY);
    }
    private void CheckMinMaxXY(Vector2Int inputMin, Vector2Int inputMax)
    {
        if (inputMax.x > FloorMaxXY.x) FloorMaxXY.x = inputMax.x;
        if (inputMax.y > FloorMaxXY.y) FloorMaxXY.y = inputMax.y;
        if (inputMin.x < FloorMinXY.x) FloorMinXY.x = inputMin.x;
        if (inputMin.y < FloorMinXY.y) FloorMinXY.y = inputMin.y;
    }
    public Vector2Int GetFloorMinXY()
    {
        return FloorMinXY;
    }
    public Vector2Int GetFloorMaxXY()
    {
        return FloorMaxXY;
    }
    public RoomData GetRoomDataByCenter(Vector2Int roomCenter)
    {
        foreach (var room in RoomByID.Values)
            if (room.center == roomCenter)
                return room;

        return null;
    }
    
}
