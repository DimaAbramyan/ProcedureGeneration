using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using UnityEditor;
public class Triangle
{
    private Vector2 circumcenter = Vector2.zero;
    private Vector2Int[] points = new Vector2Int[3];
    private Edge[] edges = new Edge[3];
    float radius;
    public Triangle(RoomData room1, RoomData room2, RoomData room3)
    {
        points = new Vector2Int[3] { room1.center, room2.center, room3.center };
    }
    
    public Triangle(Vector2Int roomCoord1, Vector2Int roomCoord2, Vector2Int roomCoord3)
    {
        points = new Vector2Int[3] { roomCoord1, roomCoord2, roomCoord3 };
        Edge[] edges = new Edge[3]
        {
            new Edge(roomCoord1, roomCoord2),
            new Edge(roomCoord1, roomCoord3),
            new Edge(roomCoord2, roomCoord3)
        };
    }
    public Edge[] GetEdges()
    {
        return edges;
    }
    public bool CheckIfPointInTriangle(Vector2 point)
    {
        if (circumcenter == Vector2.zero)
        {
            Vector2Int[] vertexs = new Vector2Int[3];
            for (int index = 0; index < 3; index++)
            {
                vertexs[index] = points[index];
            }

            float a = vertexs[1].x - vertexs[0].x;
            float b = vertexs[1].y - vertexs[0].y;
            float c = vertexs[2].x - vertexs[0].x;
            float d = vertexs[2].y - vertexs[0].y;

            float aux1 = a * (vertexs[0].x + vertexs[1].x) + b * (vertexs[0].y + vertexs[1].y);
            float aux2 = c * (vertexs[0].x + vertexs[2].x) + d * (vertexs[0].y + vertexs[2].y);
            float div = 2.0f * (a * (vertexs[2].y - vertexs[1].y) - b * (vertexs[2].x - vertexs[1].x));

            if (Math.Abs(div) < float.Epsilon)
            {
                Debug.LogError("Divided by Zero : " + div);
                return false;
            }

            circumcenter = new Vector2((d * aux1 - b * aux2) / div, (a * aux2 - c * aux1) / div);
            radius = Mathf.Sqrt((circumcenter.x - vertexs[0].x) * (circumcenter.x - vertexs[0].x) + (circumcenter.y - vertexs[0].y) * (circumcenter.y - vertexs[0].y));
        }
        if (Vector2.Distance(point, circumcenter) > radius)
        {
            return false;
        }
        return true;
    }
    public Vector2Int[] GetPoints()
    {
        return points;
    }

}
