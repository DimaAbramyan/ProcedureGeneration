using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CreateCorridors
{
    FloorContext context;
    FloorData floorData;

    public CreateCorridors(FloorContext context)
    {
        this.context = context;
    }

    public async UniTask Run()
    {
        floorData = context.floorData;
        await TryCreateCorridor(floorData);
    }

    public async UniTask TryCreateCorridor(FloorData floorData)
    {
        int max = Mathf.Max(1, System.Environment.ProcessorCount - 1);
        var semaphore = new SemaphoreSlim(max);

        var tasks = floorData.coridors.Keys.Select(async key =>
        {
            await semaphore.WaitAsync();
            try
            {
                var from = floorData.RoomByID[key.Item1];
                var to = floorData.RoomByID[key.Item2];

                await UniTask.RunOnThreadPool(() =>
                {
                    PathFindAlgorithm(from, to, Mathf.Min(from.rastLevel, to.rastLevel));
                });
            }
            finally
            {
                semaphore.Release();
            }
        });

        await UniTask.WhenAll(tasks);
    }

    void PathFindAlgorithm(RoomData fromRoom, RoomData toRoom, int rast)
    {
        bool RoomsSwapped = false;

        Vector2Int minXY = new(int.MaxValue, int.MaxValue);
        Vector2Int maxXY = new(int.MinValue, int.MinValue);

        Vector2 centersPerpend;

        HashSet<Vector2Int> usefullWallsFrom = new();
        HashSet<Vector2Int> usefullWallsTo = new();

        HashSet<Vector2Int> rasteredFromRoomWalls = new();
        HashSet<Vector2Int> rasteredToRoomWalls = new();
        HashSet<Vector2Int> obstacles = new();

        Vector2Int startCorridor = new Vector2Int(-1, -1);
        Vector2Int endCorridor = new Vector2Int(-1, -1);

        (minXY, maxXY) = MinimizeWalls();

        BuildRasterizedSearchSpace();
        SetFromAsMinimum();
        FindClosestWay();

        BuildCorridor();

        void BuildCorridor()
        {
            if (startCorridor.x < 0 || endCorridor.x < 0)
                return;

            Vector2Int a = startCorridor / rast;
            Vector2Int b = endCorridor / rast;

            foreach (var cell in TileAlgorithm.TilesOnLineSmooth(a, b))
            {
                for (int dx = 0; dx < rast; dx++)
                    for (int dy = 0; dy < rast; dy++)
                    {
                        var world = new Vector2Int(
                            cell.x * rast + dx,
                            cell.y * rast + dy
                        );

                        var key = GetCorridorKey(fromRoom.id, toRoom.id);

                        lock (floorData.coridors)
                        {
                            if (!floorData.coridors.TryGetValue(key, out var corridor))
                                continue;

                            if (corridor == null)
                            {
                                UnityEngine.Debug.LogError($"corridor is null for key {key}");
                                continue;
                            }

                            if (corridor.Tiles == null)
                            {
                                UnityEngine.Debug.LogError($"corridor.Tiles is null for key {key}, from={corridor.FromRoom?.id} to={corridor.ToRoom?.id}");
                                continue;
                            }

                            corridor.MarkCorridor(world);
                        }
                    }
            }
        }

        (Vector2Int, Vector2Int) MinimizeWalls()
        {
            Vector2 centerFrom = fromRoom.center;
            Vector2 centerTo = toRoom.center;

            Vector2 dir = centerFrom - centerTo;
            centersPerpend = new Vector2(-dir.y, dir.x).normalized;

            FindMinMax(fromRoom, true);
            FindMinMax(toRoom, false);

            return (minXY, maxXY);

            void FindMinMax(RoomData room, bool flipped)
            {
                foreach (var w in room.Walls)
                {
                    if (IsOnSide(w.Key, room.center, centersPerpend, flipped))
                    {
                        if (flipped) usefullWallsFrom.Add(w.Key);
                        else usefullWallsTo.Add(w.Key);

                        minXY = Vector2Int.Min(minXY, w.Key);
                        maxXY = Vector2Int.Max(maxXY, w.Key);
                    }
                }
            }
        }

        void SetFromAsMinimum()
        {
            if (rasteredFromRoomWalls.Count > rasteredToRoomWalls.Count)
            {
                RoomsSwapped = true;

                (rasteredFromRoomWalls, rasteredToRoomWalls) =
                    (rasteredToRoomWalls, rasteredFromRoomWalls);
            }
        }

        bool IsOnSide(Vector2 point, Vector2 linePoint, Vector2 lineDir, bool flipped)
        {
            Vector2 v = point - linePoint;
            float cross = lineDir.x * v.y - lineDir.y * v.x;
            return flipped ? cross >= 0f : cross < 0f;
        }

        void BuildRasterizedSearchSpace()
        {
            Vector2Int rMinXY = new(
                Mathf.FloorToInt((float)minXY.x / rast),
                Mathf.FloorToInt((float)minXY.y / rast)
            );

            Vector2Int rMaxXY = new(
                Mathf.FloorToInt((float)maxXY.x / rast),
                Mathf.FloorToInt((float)maxXY.y / rast)
            );

            rasteredFromRoomWalls.Clear();
            rasteredToRoomWalls.Clear();
            obstacles.Clear();

            for (int x = rMinXY.x; x <= rMaxXY.x; x++)
                for (int y = rMinXY.y; y <= rMaxXY.y; y++)
                {
                    Vector2Int cell = new(x, y);

                    bool isObstacle = false;
                    bool isFrom = false;
                    bool isTo = false;

                    for (int dx = 0; dx < rast; dx++)
                        for (int dy = 0; dy < rast; dy++)
                        {
                            Vector2Int world = new(
                                cell.x * rast + dx,
                                cell.y * rast + dy
                            );

                            var room = floorData.GetRoomByTile(world);
                            if (room == null) continue;

                            if (room != fromRoom && room != toRoom)
                                isObstacle = true;

                            if (room == fromRoom && usefullWallsFrom.Contains(world))
                                isFrom = true;

                            if (room == toRoom && usefullWallsTo.Contains(world))
                                isTo = true;
                        }

                    if (isObstacle)
                    {
                        obstacles.Add(cell);
                        continue;
                    }

                    if (isFrom) rasteredFromRoomWalls.Add(cell);
                    if (isTo) rasteredToRoomWalls.Add(cell);
                }

            minXY = rMinXY;
            maxXY = rMaxXY;
        }

        void FindClosestWay()
        {
            int bestR = int.MaxValue;
            int bestSkew = int.MaxValue;
            int maxRadius = 100;

            foreach (var from in rasteredFromRoomWalls)
            {
                for (int r = 1; r <= Mathf.Min(bestR, maxRadius); r++)
                {
                    foreach (var to in Iterate(from, r))
                    {
                        if (!rasteredToRoomWalls.Contains(to))
                            continue;
                        if (!CanConnect(from, to))
                            continue;

                        int skew = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);

                        if (r < bestR || (r == bestR && skew < bestSkew))
                        {
                            bestR = r;
                            bestSkew = skew;
                            startCorridor = from * rast;
                            endCorridor = to * rast;
                        }
                    }
                }
            }
        }

        IEnumerable<Vector2Int> Iterate(Vector2Int center, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int maxDy = radius - Mathf.Abs(dx);

                for (int dy = -maxDy; dy <= maxDy; dy++)
                {
                    Vector2Int p = center + new Vector2Int(dx, dy);

                    if (p.x < minXY.x || p.x > maxXY.x) continue;
                    if (p.y < minXY.y || p.y > maxXY.y) continue;

                    yield return p;
                }
            }
        }

        bool CanConnect(Vector2Int a, Vector2Int b)
        {
            foreach (var p in TileAlgorithm.TilesOnLine(a, b))
            {
                if (obstacles.Contains(p))
                    return false;
            }
            return true;
        }
    }

    public (int, int) GetCorridorKey(int a, int b)
    {
        return (Mathf.Min(a, b), Mathf.Max(a, b));
    }
}