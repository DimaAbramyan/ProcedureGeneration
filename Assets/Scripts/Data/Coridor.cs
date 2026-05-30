using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CoridorData
{
    public enum ConnectionType
    {
        Vertical,
        Horizontal
    }
    public RoomData FromRoom { get; }
    public RoomData ToRoom { get; }

    public HashSet<Vector2Int> startCor;
    public HashSet<Vector2Int> endCor;

    private HashSet<TileData> _tiles = new HashSet<TileData>();
    public HashSet<TileData> Tiles
    {
        get => _tiles;
        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError(
                    $"Tiles set to NULL for corridor {FromRoom?.id}->{ToRoom?.id}\n" +
                    System.Environment.StackTrace
                );
            }
            _tiles = value;
        }
    }


    public CoridorData(RoomData from, RoomData to)
    {
        FromRoom = from;
        ToRoom = to;
        Tiles = new HashSet<TileData>();

        startCor = new HashSet<Vector2Int>();
        endCor = new HashSet<Vector2Int>();

        if (Tiles == null)
            UnityEngine.Debug.LogError("Tiles null after constructor!");
    }
    public void SetStartEndCoord(HashSet<Vector2Int> start, HashSet<Vector2Int> end)
    {
        startCor = start;
        endCor = end;
    }
    public Vector3 GetMiddleStartCoord()
    {
        if (Tiles == null || Tiles.Count == 0)
            return Vector3.zero;
        Vector2 sum  = new Vector2();
        int count = 0;
        foreach (var start in startCor)
        {
            sum += start;
            count++;
        }
        return (sum / count);
    }
    public Vector3 GetMiddleEndCoord()
    {
        if (Tiles == null || Tiles.Count == 0)
            return Vector3.zero;
        Vector2 sum = new Vector2();
        int count = 0;
        foreach (var start in endCor)
        {
            sum += start;
            count++;
        }
        return (sum / count);
    }
    public void MarkCorridor(Vector2Int coridorTile)
    {
        Tiles.Add(new TileData(coridorTile,
        TileData.TileType.CorridorFloor));
    }
    
}

