using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Edge
{
    public Vector2Int a;
    public Vector2Int b;

    public Edge(Vector2Int a, Vector2Int b)
    {
        this.a = a;
        this.b = b;
    }
    public float GetDistance()
    { 
      return Vector2Int.Distance(this.a, this.b);
    }
}