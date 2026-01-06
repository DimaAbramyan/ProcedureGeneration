using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[Serializable]
public class TileData
{
    public Vector2Int coord;
    public TileType type;
    public enum TileType
    {
        Wall,
        Floor
    }
    public TileData(Vector2Int coord, TileType type)
    {
        this.coord = coord;
        this.type = type;
    }
    public TileData(Vector2Int coord)
    {
        this.coord = coord;
        type = TileType.Floor;
    }
    public TileData()
    {
        coord = new Vector2Int(0,0);
        type = TileType.Floor;
    }
}
