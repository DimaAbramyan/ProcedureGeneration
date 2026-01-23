using System.Collections.Generic;
using UnityEngine;

public class VisualiseCorridors : MonoBehaviour
{
    private FloorContext context;
    private GameObject corridorHandler;
    public Material lineMaterial;
    private List<LineRenderer> lines = new List<LineRenderer>();

    public VisualiseCorridors(FloorContext context)
    {
        this.context = context;
    }

    public void Run()
    {
        if (context == null || context.floorData == null)
            return;

        // Создаём контейнер для линий
        corridorHandler = new GameObject("Corridors");
        DrawRoomConnections();
    }

    // Преобразуем Vector2Int в Vector3 для XZ-плоскости
    private Vector3 ToVector3XZ(Vector2Int v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    private void CreateLine(Vector3 a, Vector3 b)
    {
        GameObject go = new GameObject("Line");
        var lr = go.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.startWidth = 1f;
        lr.endWidth = 1f;
        lr.useWorldSpace = true;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        go.transform.parent = corridorHandler.transform;
        lines.Add(lr);
    }

    private void DrawRoomConnections()
    {
        foreach (var fromRoom in context.floorData.rooms)
        {
            Vector3 fromCenter = ToVector3XZ(fromRoom.center); // Центр комнаты

            foreach (var toRoom in fromRoom.connectedRooms)
            {
                Vector3 toCenter = ToVector3XZ(toRoom.center);

                // Чтобы не рисовать дублирующие линии
                if (fromRoom.GetHashCode() < toRoom.GetHashCode())
                    CreateLine(fromCenter + new Vector3(0,0.1f,0), toCenter + new Vector3(0, 0.1f, 0));
            }
        }
    }
}
