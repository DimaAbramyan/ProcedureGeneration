using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelBuilder
{
    FloorData FloorData;
    [SerializeField]
    GameObject tileObj;
    GameObject TilesHandler;
    public GameObject floorHandler;
    GameObject tileObjInstance;
    private readonly FloorContext context;
    GameObject centerObj;
    Material wallMaterial;
    Material floorMaterial;
    Material corridorMaterial;

    GameObject roomHandler;

    public LevelBuilder(FloorContext context)
    {
        this.context = context;
    }

    public async void Run()
    {
        FloorData floorData = context.floorData;
        
        centerObj = Resources.Load<GameObject>("Prefabs/CenterObj");


        await BuildFloor(floorData);
    }
    public async UniTask BuildFloor(FloorData floorData)
    {
        Transform parent = GameObject.Find("LevelHandler").transform;
        FloorData = floorData;
        tileObj = Resources.Load<GameObject>("Prefabs/Floor");
        Renderer rend = tileObj.GetComponent<Renderer>();
        wallMaterial = new Material(rend.sharedMaterial);
        wallMaterial.color = Color.blue;
        floorMaterial = new Material(rend.sharedMaterial);
        floorMaterial.color = Color.green;
        corridorMaterial = new Material(rend.sharedMaterial);
        corridorMaterial.color = Color.brown;
        Material mat = tileObj.gameObject.GetComponent<Renderer>().sharedMaterial;
        
        foreach (RoomData roomData in floorData.RoomByID.Values)
        {
            Material newFloorMaterial = new Material(floorMaterial);
            newFloorMaterial.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
            roomHandler = new GameObject();
            BuildMeshFromTiles(roomData.Tiles.Values.Where(t => t.type == TileData.TileType.Floor),
                newFloorMaterial, 
                $"Floor for {roomData.id}", 
                roomHandler.transform, 
                false).transform.SetParent(parent);
            BuildMeshFromTiles(roomData.Walls.Values, 
                wallMaterial, 
                $"Room_{roomData.id}_Walls", 
                roomHandler.transform, 
                false).transform.SetParent(parent);
            await UniTask.Yield();
            roomHandler.name = $"Комната {roomData.id}-ая, кол-во тайлов:{roomData.Tiles.Count}";

        }
        foreach (CoridorData coridorData in floorData.coridors.Values)
        {
            var road= BuildMeshFromTiles(coridorData.Tiles,
                corridorMaterial,
                $"Coridor({coridorData.startCor}, {coridorData.endCor}",
                roomHandler.transform,
                false);
            road.transform.SetParent(parent);
            road.transform.position -= new Vector3(0, 0.001f);
        }
    }
    public static GameObject BuildMeshFromTiles(IEnumerable<TileData> tiles, Material material, string objectName, Transform parent, bool visual)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        int vertIndex = 0;

        foreach (var tile in tiles)
        {
            Vector3 p = new Vector3(tile.coord.x, 0, tile.coord.y);

            // вершины квадрата
            vertices.Add(p + new Vector3(0, 0, 0));
            vertices.Add(p + new Vector3(1, 0, 0));
            vertices.Add(p + new Vector3(1, 0, 1));
            vertices.Add(p + new Vector3(0, 0, 1));

            // два треугольника
            triangles.Add(vertIndex + 0);
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 1);

            triangles.Add(vertIndex + 0);
            triangles.Add(vertIndex + 3);
            triangles.Add(vertIndex + 2);

            // UV
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));

            vertIndex += 4;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject go = new GameObject(objectName);
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        var collider = go.AddComponent<MeshCollider>();

        mf.mesh = mesh;
        mr.material = material;
        collider.sharedMesh = mesh;
        collider.convex = false;
        go.transform.SetParent(parent);
        return go;
    }


    public void GenerateRoomVisual(FloorData floorData)
    {
        tileObj = Resources.Load<GameObject>("Prefabs/Tile");
        floorHandler = new GameObject();
        foreach (RoomData roomData in floorData.RoomByID.Values)
        { 
        var roomCount = floorData.RoomByID.Values.Count;
        GameObject TilesHandler = new GameObject();
        TilesHandler.name = $"{roomCount}-ая комната, содержит {roomData.Tiles.Count} тайлов";
        GameObject tileObjInstance;
            foreach (TileData tile in roomData.Tiles.Values)
            {
                tileObjInstance = Object.Instantiate(tileObj);
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
            GameObject tileObjectInstance = Object.Instantiate(centerObj, TilesHandler.transform);
            tileObjectInstance.transform.position = new Vector3(roomData.center.x, roomData.center.y, 0);
            TilesHandler.transform.parent = floorHandler.transform;
        }
    }
    public void SetFloorData(FloorData floorData)
    {FloorData = floorData; }

}
