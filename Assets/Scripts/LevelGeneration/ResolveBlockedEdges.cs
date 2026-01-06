using UnityEngine;

public class ResolveBlockedEdges
{

    private readonly FloorContext context;

    public ResolveBlockedEdges(FloorContext context)
    {
        this.context = context;
    }

    public void Run()
    {
        FloorData floorData = context.floorData;

        ResolveEdges(floorData);
    }
    void ResolveEdges(FloorData floorData)
    {
        
    }
}
