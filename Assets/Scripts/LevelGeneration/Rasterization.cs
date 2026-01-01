using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class Rasterization:MonoBehaviour
{
    Vector2Int start;
    Vector2Int end;
    public int CellSize = 8;
    public float Percent = 0.5f;
    public void RoomRasterization(RoomData cluster)
    {
        start = cluster.GetMinCoord();
        end = cluster.GetMaxCoord();

        start = new Vector2Int(Mathf.FloorToInt((float)start.x / CellSize) * CellSize,
                               Mathf.FloorToInt((float)start.y / CellSize) * CellSize);
        end = new Vector2Int(Mathf.CeilToInt((float)end.x / CellSize) * CellSize,
                               Mathf.CeilToInt((float)end.y / CellSize) * CellSize);
        float clusterPixels;
        for (int i = start.x; i < end.x; i += CellSize)
        {
            clusterPixels = 0;
            for (int j = start.y; j < end.y; j += CellSize)
            {
                //Debug.Log($"SEX: {start.x % CellSize == 0 && start.y % CellSize == 0 && end.x % CellSize == 0 && end.y % CellSize == 0}");
                Vector2Int from = new Vector2Int(i, j);
                Vector2Int to = new Vector2Int(i + CellSize, j +CellSize);
                //Debug.Log("Начало: " + from + " конец: " + to);
                clusterPixels = cluster.CountSquare(from, to);
                if (clusterPixels / Mathf.Pow((float)CellSize, 2) >= Percent)
                {
                    cluster.FillCell(from, to);
                }
                else
                {
                    cluster.ClearCell(from, to);
                }
                //Debug.Log("ААААААААААААА " + (clusterPixels / Mathf.Pow((float)CellSize, 2)));
                //Debug.Log(($"Проверка: {((clusterPixels / ((float)CellSize * (float)CellSize)) >= Percent)} кол-во пикселей, которые подходят: {clusterPixels} "));
            }
        }
        cluster.CountCenter();
    }

}
