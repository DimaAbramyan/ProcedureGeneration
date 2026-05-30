using UnityEngine;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using System;

public class RoomGenerator
{
    public Rasterization rasterization;
    private RoomData center;
    private TriangulationGenerator triangulationDelone;
    private bool CreateVisual = true;
    public float fromColor;
    public float toColor;
    public FloorData floorData { get; private set; }
    private RoomData roomData;

    private bool[,] visited;

    Color[] pixels;
    int width;
    int height;
    private int roomCount;
    private  FloorContext context;

    public RoomGenerator(FloorContext context)
    {
        this.context = context;
        this.rasterization = context.rasterization;
    }


    public void Run()
    {
        //GenerationTimer.Watch.Start();
        floorData = context.floorData;
        if (context.source == null)
        {
            Debug.LogError("context.source == null! Назначьте CellularTextureApplier перед Run()");
            return;
        }

        fromColor = context.fromColor;
        toColor = context.toColor;

        pixels = context.mapColor;
        width = context.mapWidht;
        height = context.mapHeight;

        Generate(floorData);
        //Debug.Log(
        //    $"1-ая генерация: {GenerationTimer.Watch.ElapsedMilliseconds} ms"
        //);
        //GenerationTimer.Watch.Stop();

    }

    public void Generate(FloorData floorData)
    {
        int size = context.source.GetTextureSize();

        visited = new bool[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int index = y * width + x;
                if (visited[x, y])
                    continue;


                if (ColourComparison.ColourCheck(pixels[index], x => x >= fromColor && x < toColor))
                {
                    List<Vector2Int> cluster = FloodFill(x, y);

                    CreateRoom(cluster);
                }
            }
        }
        //context = new FloorContext(floorData);
        //ResolveBlockedEdges floorData = new ResolveBlockedEdges();
    }

    List<Vector2Int> FloodFill(int startX, int startY)
    {
        int size = context.source.GetTextureSize();

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
                int index = ny * size + nx;
                if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                    continue;

                if (visited[nx, ny])
                    continue;

                if (ColourComparison.ColourCheck(pixels[index], x => x >= fromColor && x < toColor))
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
        roomData = new RoomData(floorData);
        Vector2Int sum = Vector2Int.zero;
        foreach (var p in cluster) sum += p;
        Vector2Int centerPos = sum / cluster.Count;
        foreach (var r in cluster)
        {
            roomData.AddTile(new TileData(r));
        }
        if (roomData.Tiles.Count == 0)
        {
            return;
        }
       
        roomData.id = roomCount;
        roomCount++;

        floorData.AddRoom(roomData);
        roomData.RecountRasterizationLevel();


        Debug.Log($"Комната {roomCount}. Пикселей: {cluster.Count}, центр: {centerPos}, уровень растеризации: {roomData.rastLevel}");
    }
}
