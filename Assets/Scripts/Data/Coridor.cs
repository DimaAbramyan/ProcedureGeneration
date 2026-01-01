using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coridor
{
    public Vector2[] connectedPoints = new Vector2[2];

    public void  AddCoridor(TileData from, TileData to)
    {
        connectedPoints[0] = from.coord;
        connectedPoints[1] = to.coord;
    }
    public Coridor()
    {
        connectedPoints = new Vector2[2];
    }
}
