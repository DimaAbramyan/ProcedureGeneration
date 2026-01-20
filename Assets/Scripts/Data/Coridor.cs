using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coridor
{
    public Vector2Int[] connectedPoints = new Vector2Int[2];
    public float lenght;
    public Coridor(Vector2Int from, Vector2Int to)
    {
        connectedPoints[0] = from;
        connectedPoints[1] = to;
        lenght = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
}
