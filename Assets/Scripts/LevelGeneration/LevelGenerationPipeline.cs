using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Diagnostics;
public class FloorGenerationPipeline
{
    private readonly FloorContext context;

    public FloorGenerationPipeline(FloorContext context)
    {
        this.context = context;
    }

    public async UniTask GenerateAsync()
    {
        GenerationTimer.Watch.Restart();
        var noiseMap = context.source.NoiseMap;

        context.mapColor = noiseMap.GetPixels();
        context.mapWidht = noiseMap.width;
        context.mapHeight = noiseMap.height;

        var sb = new System.Text.StringBuilder();
        var sw = new Stopwatch();

        await UniTask.RunOnThreadPool(async() =>
        {
            sw.Restart();
            new RoomGenerator(context).Run();
            sw.Stop();
            sb.AppendLine($"RoomGenerator: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            new Rasterization(context).Run();
            sw.Stop();
            sb.AppendLine($"Rasterization: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            new TriangulationGenerator(context).Run();
            sw.Stop();
            sb.AppendLine($"TriangulationGenerator: {sw.ElapsedMilliseconds} ms");
            
            sw.Restart();
            await new DeleteImpossibleCorridors(context).Run();
            sw.Stop();
            sb.AppendLine($"ResolveBlockedEdges: {sw.ElapsedMilliseconds} ms");

            UnityEngine.Debug.Log("Начали строить коридоры");
            sw.Restart();
            await new CreateCorridors(context).Run();
            sw.Stop();
            sb.AppendLine($"CreateCorridors: {sw.ElapsedMilliseconds} ms");

            UnityEngine.Debug.Log("Закончили строить коридоры");
            sw.Restart();
            new MinOstTreeGenerator(context).Run();
            sw.Stop();
            sb.AppendLine($"MinOstTreeGenerator: {sw.ElapsedMilliseconds} ms");

           });


        await UniTask.SwitchToMainThread();
        sw.Restart();
        new LevelBuilder(context).Run();
        sw.Stop();
        sb.AppendLine($"LevelBuilder: {sw.ElapsedMilliseconds} ms");

        sw.Restart();
        new VisualiseCorridors(context).Run();
        sw.Stop();
        sb.AppendLine($"VisualiseCorridors: {sw.ElapsedMilliseconds} ms");

        UnityEngine.Debug.Log(sb.ToString());
        foreach (var room in context.floorData.RoomByID)
        {
            UnityEngine.Debug.Log(room.Key);
        }
        UnityEngine.Debug.Log(context.floorData.RoomByID.Count);

    }
}
