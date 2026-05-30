using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Edge
{
    public Vector2Int A;
    public Vector2Int B;


    public float Weight;

    public Edge(Vector2Int a, Vector2Int b)
    {
        A = a;
        B = b;

        Weight = Vector2Int.Distance(a, b);
    }
    public float GetDistance()
    { 
      return Vector2Int.Distance(A, B);
    }
}