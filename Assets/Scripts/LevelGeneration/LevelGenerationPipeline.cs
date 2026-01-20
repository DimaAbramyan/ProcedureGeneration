using System.Collections.Generic;
using UnityEngine;

public class FloorGenerationPipeline
{
    private readonly FloorContext context;

    public FloorGenerationPipeline(FloorContext context)
    {
        this.context = context;
    }

    public void Generate()
    {
        GenerationTimer.Watch.Restart();
        new RoomGenerator(context).Run();
        new Rasterization(context).Run();
        new TriangulationGenerator(context).Run();
        new ResolveBlockedEdges(context).Run();
        new MinOstTreeGenerator(context).Run();
        new LevelBuilder(context).Run();
        Debug.Log(
            $"Generation time: {GenerationTimer.Watch.ElapsedMilliseconds} ms"
        );
        GenerationTimer.Watch.Stop();
    }
}
