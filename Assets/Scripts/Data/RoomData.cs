using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomData
{
    public int number;
    public int rastLevel;
    public Dictionary<Vector2Int, TileData> Tiles = new();
    public Dictionary<Vector2Int, TileData> Walls = new();
    public Vector2Int center { get; private set; }
    public Vector2Int MinTileXY { get; private set; }
    public Vector2Int MaxTileXY { get; private set; }
    public Dictionary<RoomData, CoridorData> coridors;

    public RoomData()
    {
        Tiles = new Dictionary<Vector2Int, TileData>();
        center = new Vector2Int(0,0);
        MinTileXY = new Vector2Int(int.MaxValue, int.MaxValue);
        MaxTileXY = new Vector2Int(int.MinValue, int.MinValue);
        rastLevel = 1;
        coridors = new Dictionary<RoomData, CoridorData>();
    }
    public RoomData(Vector2Int centerCoord)
    {
        Tiles = new Dictionary<Vector2Int, TileData>();
        center = centerCoord;
        MinTileXY = new Vector2Int(int.MaxValue, int.MaxValue);
        MaxTileXY = new Vector2Int(int.MinValue, int.MinValue);
        coridors = new Dictionary<RoomData, CoridorData>();
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
    public void RemoveTile(Vector2Int tileCoord)
    {
        if (Tiles.ContainsKey(tileCoord))
        {
            if (Walls.ContainsKey(tileCoord))
            {
                Walls.Remove(tileCoord);
            }
            Tiles.Remove(tileCoord);
            }
        CountCenter();
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
                {
                    AddTile(new TileData(key));
                }
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
                {
                    Tiles.Remove(key);
                    Walls.Remove(key);
                }
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
                    if (!Walls.ContainsKey(coord))
                    Walls.Add(coord, tile);
                }
            }
        }
    }
    public void RecountRasterizationLevel()
    {
        int TilesCount = Tiles.Count;
        if (TilesCount > 400 && TilesCount <= 1200)
        {
            rastLevel = 2;
        }
        if (TilesCount > 1200 && TilesCount <= 2800)
        {
            rastLevel = 4;
        }
        if ((TilesCount > 2800) && (TilesCount <= 37600)) 
        {
            rastLevel = 8;
        }
        if (TilesCount > 37600)
        {
            rastLevel = 16;
        }
    }
    bool IsTileFloor(Vector2Int TileToCheck)
    {
        return (Tiles.ContainsKey(new Vector2Int(TileToCheck.x + 1, TileToCheck.y)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x - 1, TileToCheck.y)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x, TileToCheck.y + 1)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x, TileToCheck.y - 1))) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x+1, TileToCheck.y+1)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x + 1, TileToCheck.y - 1)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x - 1, TileToCheck.y + 1)) &&
               Tiles.ContainsKey(new Vector2Int(TileToCheck.x - 1, TileToCheck.y - 1));
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
        Walls.Clear();
        MinTileXY = new Vector2Int(int.MaxValue, int.MaxValue);
        MaxTileXY = new Vector2Int(int.MinValue, int.MinValue);

        center = Vector2Int.zero;

        Debug.Log($"Удалено {count} тайлов");
    }
    public void AddConectedRoom(RoomData toRoom)
    {
        if (toRoom == null)
            return;

        if (coridors.ContainsKey(toRoom))
            return;

        var corridor = new CoridorData(this, toRoom);
        coridors.Add(toRoom, corridor);
    }


    public void RemoveConnectedRoom(RoomData newConectedRoom)
    {
        coridors.Remove(newConectedRoom);
    }
}
