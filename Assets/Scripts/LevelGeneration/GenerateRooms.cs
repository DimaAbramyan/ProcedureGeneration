using UnityEngine;
using System.Collections.Generic;

public class GenerateRooms : MonoBehaviour
{
    [SerializeField] public Rasterization rasterization;
    [SerializeField] private RoomData center;
    [SerializeField] private CellularTextureApplier source;
    [SerializeField] private TriangulationDelone triangulationDelone;
    [SerializeField] private bool GenerateFloor = false;
    [SerializeField] public float fromColor;
    [SerializeField] public float toColor;


    public FloorData floorData { get; private set; }
    private RoomData roomData;

    private Texture2D map;
    private bool[,] visited;

    private GameObject tileObj;
    private GameObject centerObj;
    private GameObject floorHandler;

    private int roomCount;
    public void Generate()
    {
        Debug.Log($"От {fromColor}, До {toColor}");
        floorHandler = new GameObject();
        floorData = new FloorData();
        tileObj = Resources.Load<GameObject>("Prefabs/Tile");
        centerObj = Resources.Load<GameObject>("Prefabs/CenterObj");

        map = source.NoiseMap;
        int size = source.GetTextureSize();

        visited = new bool[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (visited[x, y])
                    continue;


                if (ColourComparison.ColourCheck(map.GetPixel(x,y), x => x >= fromColor && x < toColor))
                {
                    List<Vector2Int> cluster = FloodFill(x, y);

                    CreateRoom(cluster);
                }
            }
        }
        triangulationDelone.Init(floorData);
        triangulationDelone.Triangulation();
    }

    List<Vector2Int> FloodFill(int startX, int startY)
    {
        int size = source.GetTextureSize();

        List<Vector2Int> cluster = new List<Vector2Int>();
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        q.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (q.Count > 0)
        {
            Vector2Int p = q.Dequeue();
            cluster.Add(p);

            foreach (var n in Neighbors(p))
            {
                int nx = n.x;
                int ny = n.y;

                if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                    continue;

                if (visited[nx, ny])
                    continue;

                if (ColourComparison.ColourCheck(map.GetPixel(nx, ny), x => x >= fromColor && x < toColor))
                {
                    visited[nx, ny] = true;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return cluster;
    }

    IEnumerable<Vector2Int> Neighbors(Vector2Int p)
    {
        yield return new Vector2Int(p.x + 1, p.y);
        yield return new Vector2Int(p.x - 1, p.y);
        yield return new Vector2Int(p.x, p.y + 1);
        yield return new Vector2Int(p.x, p.y - 1);
    }

    void CreateRoom(List<Vector2Int> cluster)
    {
        roomData = new RoomData();
        Vector2 sum = Vector2.zero;
        foreach (var p in cluster) sum += p;
        Vector2 centerPos = sum / cluster.Count;

        if (GenerateFloor)
        {
            foreach (var r in cluster)
            {
                roomData.AddTile(new TileData(r));
            }
        }
        rasterization.RoomRasterization(roomData);
        if (roomData.Tiles.Count == 0)
        {
            return;
        }
        roomData.MakeWalls(roomData.GetMinCoord(), roomData.GetMaxCoord());
        GenerateRoomVisual();
        roomCount++;
        roomData.number = roomCount;
        floorData.AddRoom(roomData);
        Debug.Log($"Комната {roomCount}. Пикселей: {cluster.Count}, центр: {centerPos}");
    }

    public void GenerateRoomVisual()
    {
        GameObject TilesHandler = new GameObject();
        TilesHandler.name = $"{roomCount}-ая комната, содержит {roomData.Tiles.Count} тайлов";
        GameObject tileObjInstance;
        
        foreach (TileData tile in roomData.Tiles.Values)
        {
            tileObjInstance = Instantiate(tileObj);
            tileObjInstance.transform.position = new Vector3(tile.coord.x, tile.coord.y);
            tileObjInstance.transform.parent = TilesHandler.transform;
            if (tile.type == TileData.TileType.Floor)
            {
                tileObjInstance.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                tileObjInstance.GetComponent<Renderer>().material.color = Color.blue;
            }
        }
        GameObject tileObjectInstance = Instantiate(centerObj, TilesHandler.transform);
        tileObjectInstance.transform.position = new Vector3(roomData.center.x, roomData.center.y, 0);
        TilesHandler.transform.parent = floorHandler.transform;
    }
    public void ClearFloor()
    {
        if (floorHandler == null)
            return;

        Destroy(floorHandler.gameObject);
        for (int i = 0; i < floorData.rooms.Count; i++)
        {
            floorData.rooms[i].DestroyRoom();
        }
        roomCount = 0;
    }
}
