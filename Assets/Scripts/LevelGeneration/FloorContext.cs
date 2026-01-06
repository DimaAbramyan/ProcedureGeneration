using UnityEngine;

public class FloorContext
{
    public FloorData floorData;
    public Rasterization rasterization;
    public CellularTextureApplier source;
    public GameObject tilePrefab;
    public float fromColor;
    public float toColor;
    public uint seed;

    public FloorContext()
    {
        floorData = new FloorData();
    }
}
