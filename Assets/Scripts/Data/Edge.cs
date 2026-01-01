using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Edge
{
    public Vector2 a;
    public Vector2 b;

    public Edge(Vector2 a, Vector2 b)
    {
        this.a = a;
        this.b = b;
    }
    public float GetDistance()
    { 
      return Vector2.Distance(this.a, this.b);
    }
}