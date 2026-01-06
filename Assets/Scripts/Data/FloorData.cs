using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Unity.VisualScripting;
using UnityEngine;
public class FloorData
{
    public List<RoomData> rooms;
    public List<Coridor> coridors;
    public int FloorCount;
    Vector2Int FloorMaxXY;
    Vector2Int FloorMinXY;
    public FloorData(List<RoomData> clusters)
    {
        foreach (RoomData cluster in clusters)
        {
            if (cluster == null) continue;
            rooms.Add(cluster);
            CheckMinMaxXY(cluster.MinTileXY, cluster.MaxTileXY);
        }
    }
    public FloorData()
    {
        rooms = new List<RoomData>();
        coridors = new List<Coridor>();

        FloorMaxXY = new Vector2Int(int.MinValue, int.MinValue);
        FloorMinXY = new Vector2Int(int.MaxValue, int.MaxValue);
    }

    public void AddRoom(RoomData Room)
    {
        rooms.Add(Room);
        CheckMinMaxXY(Room.MinTileXY, Room.MaxTileXY);
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
        foreach (var room in rooms)
        {
            if (room.center == roomCenter)
                    {
                return room;
            }
        }
        return null;
    }
}
