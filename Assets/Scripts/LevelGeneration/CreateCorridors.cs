using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
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
        //MinimizeWalls(floorData.rooms[4], floorData.rooms[1]);
        //MinimizeWalls(floorData.rooms[7], floorData.rooms[10]);
        //MinimizeWalls(floorData.rooms[25], floorData.rooms[17]);
        //MinimizeWalls(floorData.rooms[0], floorData.rooms[20]);
    }
    public async UniTask TryCreateCorridor(FloorData floorData)
    {
        var tasks = new List<UniTask>();
        
        foreach (RoomData fromRoom in floorData.rooms)
        {
            foreach (RoomData toRoom in fromRoom.coridors.Keys)
            {
                var from = fromRoom;
                var to = toRoom;

                tasks.Add(
                    UniTask.RunOnThreadPool(() =>
                    {
                        PathFindAlgorithm(from, to, Mathf.Min(from.rastLevel, to.rastLevel));
                    })
                );
            }
        }

        await UniTask.WhenAll(tasks);
    }
    void PathFindAlgorithm(RoomData fromRoom, RoomData toRoom, int rast)
    {
        Vector2 centersPerpend;

        Vector2Int minXY = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int maxXY = new Vector2Int(int.MinValue, int.MinValue);

        HashSet<Vector2Int> usefullWallsFrom = new HashSet<Vector2Int>();
        HashSet<Vector2Int> usefullWallsTo = new HashSet<Vector2Int>();

        HashSet<Vector2Int> rasteredFromRoomWalls = new HashSet<Vector2Int>();
        HashSet<Vector2Int> rasteredToRoomWalls = new HashSet<Vector2Int>();
        HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>();

        Vector2Int startCorridor = new Vector2Int();
        Vector2Int endCorridor = new Vector2Int();

        (minXY, maxXY) = MinimizeWalls();
        BuildRasterizedSearchSpace();
        SetFromAsMinimum();
        FindClosestWay();
        Debug.Log($"Из конматы {fromRoom.number} в {toRoom.number} начало корридора: {startCorridor}, конец:{endCorridor}");
        (Vector2Int minXY, Vector2Int maxXY) MinimizeWalls()
        {
            Vector2Int centerFrom = fromRoom.center;
            Vector2Int centerTo = toRoom.center;
            Vector2Int centersConnection = centerFrom - centerTo;

            centersPerpend = new Vector2(-centersConnection.y, centersConnection.x).normalized;
            float angle = Mathf.Atan2(centersConnection.y, centersConnection.x) * Mathf.Rad2Deg + 180;

            //Debug.Log($"{fromRoom.number}, {toRoom.number}, угол между ними {angle}");
            FindMinMax(fromRoom, true);
            FindMinMax(toRoom, false);
            return (minXY, maxXY);



            void FindMinMax(RoomData room, bool flippedRooms)
            {
                var checkingWalls = room.Walls.ToList();
                foreach (var wall in checkingWalls)
                {
                    if (IsOnSide(wall.Key, room.center, centersPerpend, flippedRooms))
                    {
                        if (flippedRooms)
                            usefullWallsFrom.Add(wall.Key);
                        else
                            usefullWallsTo.Add(wall.Key);
                        CheckNewMinMaxXY(wall.Key);
                    }
                }



                void CheckNewMinMaxXY(Vector2Int coordChecking)
                {
                    if (coordChecking.x < minXY.x)
                    {
                        minXY.x = coordChecking.x;
                    }
                    if (coordChecking.y < minXY.y)
                    {
                        minXY.y = coordChecking.y;
                    }
                    if (coordChecking.x > maxXY.x)
                    {
                        maxXY.x = coordChecking.x;
                    }
                    if (coordChecking.y > maxXY.y)
                    {
                        maxXY.y = coordChecking.y;
                    }
                }
            }




        }
        void SetFromAsMinimum()
        {
            if (rasteredFromRoomWalls.Count > rasteredToRoomWalls.Count)
            {
                (rasteredFromRoomWalls, rasteredToRoomWalls) = (rasteredToRoomWalls, rasteredFromRoomWalls);
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
            for (int x = minXY.x; x <= maxXY.x; ++x)
            {
                for (int y = minXY.y; y <= maxXY.y; ++y)
                {
                    Vector2Int tileCoord = new Vector2Int(x, y);
                    RoomData checkedRoom = floorData.GetRoomByTile(tileCoord);
                    if (checkedRoom == null)
                        continue;
                    if ((checkedRoom != fromRoom) && (checkedRoom != toRoom))
                    {
                        obstacles.Add(tileCoord / rast);
                        continue;
                    }
                    if (checkedRoom == fromRoom && usefullWallsFrom.Contains(tileCoord))
                    {
                        rasteredFromRoomWalls.Add(tileCoord / rast);
                        continue;
                    }
                    if (checkedRoom == toRoom && usefullWallsTo.Contains(tileCoord))
                    {
                        rasteredToRoomWalls.Add(tileCoord / rast);
                        continue;
                    }
                }
            }
            minXY = minXY / rast;
            maxXY = maxXY / rast;
        }
        void FindClosestWay()
        {
            int maxRombSize = 75;
            foreach (var fromWall in rasteredFromRoomWalls)
            {
                int rombSize = 1;
                while (rombSize <= maxRombSize)
                {
                    foreach (var tile in IterateManhattanDiamondClamped(fromWall, rombSize))
                    {
                        if (rasteredToRoomWalls.Contains(tile))
                        {
                            if (CanConnectWalls(fromWall, tile))
                            {
                                maxRombSize = rombSize;
                                startCorridor = fromWall * rast;
                                endCorridor = tile * rast;
                                break;
                            }
                        }
                    }
                    rombSize++;
                }
            }

            if (fromRoom.coridors.TryGetValue(toRoom, out CoridorData corridor))
            {
                corridor.SetStartEndCoord(GetAllTiles(startCorridor), GetAllTiles(endCorridor));
            }



            IEnumerable<Vector2Int> IterateManhattanDiamondClamped(Vector2Int center,int radius)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int maxDy = radius - Mathf.Abs(dx);

                    int x = center.x + dx;
                    if (x < minXY.x || x > maxXY.x)
                        continue;

                    for (int dy = -maxDy; dy <= maxDy; dy++)
                    {
                        int y = center.y + dy;
                        if (y < minXY.y || y > maxXY.y)
                            continue;

                        yield return new Vector2Int(x, y);
                    }
                }
            }



            bool CanConnectWalls(Vector2Int from, Vector2Int to)
            {
                foreach (var tilePos in TileAlgorithm.TilesOnLine(from, to))
                {
                    if (obstacles.Contains(tilePos) || rasteredFromRoomWalls.Contains(tilePos))
                        return false;
                }
                return true;
            }



            HashSet<Vector2Int> GetAllTiles(Vector2Int tile)
            {
                HashSet<Vector2Int> result = new HashSet<Vector2Int>();
                for (int i = 0; i < rast; i++)
                {
                    for (int j = 0; j < rast; j++)
                    {
                        result.Add(tile + new Vector2Int(i,j));
                    }
                }
                return result;
            }



        }
    }
}
