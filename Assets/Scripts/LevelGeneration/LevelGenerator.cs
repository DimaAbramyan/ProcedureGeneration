using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator 
{
    FloorData FloorData;
    [SerializeField]
    GameObject tileObj;
    GameObject TilesHandler;
    public GameObject floorHandler;
    GameObject tileObjInstance;
    private readonly FloorContext context;

    public LevelGenerator(FloorContext context)
    {
        this.context = context;
    }

    public void Run()
    {
        FloorData floorData = context.floorData;

        GenerateFloor(floorData);
    }
    public void GenerateFloor(FloorData floorData)
    {
        FloorData = floorData;
        TilesHandler = new GameObject();
        tileObj = Resources.Load<GameObject>("Prefabs/Floor");
        foreach (RoomData roomData in floorData.rooms)
        {
            TilesHandler.transform.position = new Vector3(roomData.center.x, 0, roomData.center.y);
            foreach (TileData tile in roomData.Tiles.Values)
            {
                tileObjInstance = Object.Instantiate(tileObj);
                //tileObjInstance.transform.SetParent(TilesHandler.transform, true);
                tileObjInstance.transform.position = new Vector3(tile.coord.x, 0, tile.coord.y);
                if (tile.type == TileData.TileType.Floor)
                {
                        tileObjInstance.GetComponent<Renderer>().material.color = Color.green;
                    }
                    else
                    {
                        tileObjInstance.GetComponent<Renderer>().material.color = Color.blue;
                    }
            }
        }
    }

    public void SetFloorData(FloorData floorData)
    {FloorData = floorData; }

}
