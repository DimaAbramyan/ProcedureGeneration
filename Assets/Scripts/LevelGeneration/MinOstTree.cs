using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinOstTreeGenerator
{
    [SerializeField]
    LevelGenerator levelGenerator;
    FloorData floorData;
    List<Triangle> triangles;
    Edge[] edges;
    private readonly FloorContext context;

    public MinOstTreeGenerator(FloorContext context)
    {
        this.context = context;
    }

    public void Run()
    {
        FloorData floorData = context.floorData;

        FindMinOstTree(floorData);
    }
    private void FindMinOstTree(FloorData floorData)
    {
        //foreach (Triangle t in triangles)
        //{
        //    edges = t.GetEdges();
        //    foreach(Edge edge in edges)
        //    {

        //    }
        //}
        //levelGenerator.GenerateFloor(floorData);
    }
    public void SetValue(List<Triangle> triangles)
    {
        this.triangles = triangles;
    }
    public void GetFloorData(FloorData floorData)
    {
        this.floorData = floorData;
    }
}
