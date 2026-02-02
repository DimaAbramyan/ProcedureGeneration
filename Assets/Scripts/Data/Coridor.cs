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
    public HashSet<Vector2Int> Tiles;

    public ConnectionType startConnection;
    public ConnectionType endConnection;

    public CoridorData(RoomData from, RoomData to)
    {
        FromRoom = from;
        ToRoom = to;
    }
    public void SetStartEndCoord(HashSet<Vector2Int> start, HashSet<Vector2Int> end)
    {
        startCor = start;
        endCor = end;
    }
    public Vector3 GetMiddleStartCoord()
    {
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
        Vector2 sum = new Vector2();
        int count = 0;
        foreach (var start in endCor)
        {
            sum += start;
            count++;
        }
        return (sum / count);
    }

}

