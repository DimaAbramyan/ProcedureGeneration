using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomData
{
    public int number;
    public Dictionary<Vector2Int, TileData> Tiles = new();
    public List<Vector2Int> Walls = new();
    public Vector2Int center { get; private set; }
    public Vector2Int MinTileXY { get; private set; }
    public Vector2Int MaxTileXY { get; private set; }
    public HashSet<RoomData> connectedRooms;

    public RoomData()
    {
        Tiles = new Dictionary<Vector2Int, TileData>();
        center = new Vector2Int(0,0);
        MinTileXY = new Vector2Int(int.MaxValue, int.MaxValue);
        MaxTileXY = new Vector2Int(int.MinValue, int.MinValue);
        connectedRooms = new HashSet<RoomData>();
    }
    public RoomData(Vector2Int centerCoord)
    {
        Tiles = new Dictionary<Vector2Int, TileData>();
        center = centerCoord;
        MinTileXY = new Vector2Int(int.MaxValue, int.MaxValue);
        MaxTileXY = new Vector2Int(int.MinValue, int.MinValue);
        connectedRooms = new HashSet<RoomData>();
    }
    public void AddTile(TileData tile)
    {
        if (!Tiles.ContainsKey(tile.coord))
        {
            Tiles[tile.coord] = tile;

            if (tile.coord.x <= MinTileXY.x) MinTileXY = new Vector2Int(tile.coord.x, MinTileXY.y);
            if (tile.coord.y <= MinTileXY.y) MinTileXY = new Vector2Int(MinTileXY.x, tile.coord.y);
            if (tile.coord.x >= MaxTileXY.x) MaxTileXY = new Vector2Int(tile.coord.x+1, MaxTileXY.y);
            if (tile.coord.y >= MaxTileXY.y) MaxTileXY = new Vector2Int(MaxTileXY.x, tile.coord.y+1);
        }
    }

    public int CountSquare(Vector2Int from, Vector2Int to)
    {
        int result = 0;
        for (int x = from.x; x < to.x; x++)
            for (int y = from.y; y < to.y; y++)
                if (Tiles.ContainsKey(new Vector2Int(x, y)))
                    result++;
        return result;
    }
    public Vector2Int GetMinCoord()
    {
        return MinTileXY;
    }
    public Vector2Int GetMaxCoord()
    {
        return MaxTileXY;
    }
    public void FillCell(Vector2Int from, Vector2Int to)
    {
        for (int i = from.x; i < to.x; i++)
        {
            for (int j = from.y; j < to.y; j++)
            {
                Vector2Int key = new Vector2Int(i, j);
                if (!Tiles.ContainsKey(key))
                    AddTile(new TileData(key));
            }
        }
    }
    public void ClearCell(Vector2Int from, Vector2Int to)
    {
        for (int i = from.x; i < to.x; i++)
        {
            for (int j = from.y; j < to.y; j++)
            {
                Vector2Int key = new Vector2Int(i, j);
                if (Tiles.ContainsKey(key))
                    Tiles.Remove(key);
            }
        }
    }

    public void MakeWalls(Vector2Int from, Vector2Int to)
    {
        for (int x = from.x; x < to.x; x++)
        {
            for (int y = from.y; y < to.y; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);

                if (!Tiles.TryGetValue(coord, out TileData tile))
                    continue;

                if (IsTileFloor(coord))
                {
                    tile.type = TileData.TileType.Floor;
                }
                else
                {
                    tile.type = TileData.TileType.Wall;
                    Walls.Add(coord);
                }
            }
        }
    }
    bool IsTileFloor(Vector2Int TileToCheck)
    {
        return Tiles.ContainsKey(new Vector2Int(TileToCheck.x + 1, TileToCheck.y)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x - 1, TileToCheck.y)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x, TileToCheck.y + 1)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x, TileToCheck.y - 1));
    }
    public void CountCenter()
    {
        Vector2Int coordSum = new Vector2Int(0, 0);
        int count = 0;
        foreach (var tile in Tiles)
        {
            coordSum += tile.Key;
            count++;
        }
        if (count > 0)
        center = new Vector2Int(coordSum.x/count, coordSum.y/count);
    }
    public void DestroyRoom()
    {
        int count = Tiles.Count;
        Tiles.Clear();
        MinTileXY = new Vector2Int(int.MaxValue, int.MaxValue);
        MaxTileXY = new Vector2Int(int.MinValue, int.MinValue);

        center = Vector2Int.zero;

        Debug.Log($"Удалено {count} тайлов");
    }
    public void AddConectedRoom(RoomData newConectedRoom)
    {
        connectedRooms.Add(newConectedRoom);
    }
}
