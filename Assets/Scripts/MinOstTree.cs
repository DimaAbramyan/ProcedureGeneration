using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinOstTree : MonoBehaviour
{
    List<Triangle> triangles;
    Edge[] edges;
    public void Init()
    {
        
    }
    private void FindMinOstTree()
    {
        foreach (Triangle t in triangles)
        {
            edges = t.GetEdges();
            foreach(Edge edge in edges)
            {

            }
        }
    }
    public void SetValue(List<Triangle> triangles)
    {
        this.triangles = triangles;
    }
}
