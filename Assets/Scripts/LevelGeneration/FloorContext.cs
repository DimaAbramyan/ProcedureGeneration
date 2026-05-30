using System.Collections.Generic;
using UnityEngine;

public class FloorContext
{
    public FloorData floorData;
    public Rasterization rasterization;
    public CellularTextureApplier source;
    public GameObject tilePrefab;
    public List<Triangle> triangles = new();

    public float fromColor;
    public float toColor;
    public uint seed;
    public Color[] mapColor;
    public int mapWidht;
    public int mapHeight;
    public float coridorPercent;

    public FloorContext()
    {
        floorData = new FloorData();
    }
    public List<CoridorData> GetAllCoridors()
    {
        HashSet<CoridorData> result = new HashSet<CoridorData>();

        foreach (var room in floorData.RoomByID.Values)
        {
            foreach (var corridor in room.floor.coridors.Values)
            {
                if (corridor == null)
                    continue;

                result.Add(corridor);
            }
        }

        return new List<CoridorData>(result);
    }
}
