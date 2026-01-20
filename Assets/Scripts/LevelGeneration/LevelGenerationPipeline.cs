using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        await UniTask.RunOnThreadPool(() =>
        {
            new RoomGenerator(context).Run();
            new Rasterization(context).Run();
            new TriangulationGenerator(context).Run();
            new ResolveBlockedEdges(context).Run();
            new MinOstTreeGenerator(context).Run();
        });

        // Возвращаемся в main thread
        await UniTask.SwitchToMainThread();

        // Unity-часть
        new LevelBuilder(context).Run();

        Debug.Log(
            $"Generation time: {GenerationTimer.Watch.ElapsedMilliseconds} ms"
        );
        GenerationTimer.Watch.Stop();
    }
}
