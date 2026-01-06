public class FloorGenerationPipeline
{
    private readonly FloorContext context;

    public FloorGenerationPipeline()
    {
        context = new FloorContext
        {
            floorData = new FloorData()
        };
    }

    public void Generate()
    {
        new RoomGenerator(context).Run();
        new Rasterization(context).Run();
        new TriangulationGenerator(context).Run();
        new ResolveBlockedEdges(context).Run();
        new MinOstTreeGenerator(context).Run();
        new LevelGenerator(context).Run();
    }
}
