using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    private const int moveStraightCost = 10;
    private const int moveDiagonalCost = 14;


    [SerializeField] Map map;
    [SerializeField] bool platformer;

    private List<Tile> openList;
    private List<Tile> closedList;

    public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        Vector2Int StartPos = map.GetXY(new Vector3(startWorldPos.x, startWorldPos.y));
        Vector2Int EndPos = map.GetXY(new Vector3(endWorldPos.x, endWorldPos.y));

        List<Tile> path = FindPath(StartPos.x, StartPos.y, EndPos.x, EndPos.y);
        if (path == null)
            return null;
        else
        {
            List<Vector3> VPath = new List<Vector3>();
            foreach (Tile tile in path)
            {
                VPath.Add(new Vector3(tile.worldX + map.cellSize / 2, tile.worldY + map.cellSize / 2));
            }
            return VPath;
        }
    }

    public List<Tile> FindPath(int startX, int startY, int endX, int endY)
    {
        if (platformer)
        {
            //returns null if - no map exists, starting tile isnt walkable, end tile isnt walkable, starting position is bottom of map, space below player start isnt walkable
            if (map.tileMap == null || !map.tileMap[startY * map.width + startX].isWalkable || !map.tileMap[endY * map.width + endX].isWalkable || startY == 0 ||
                ((startY - 1) >= 0 && map.tileMap[(startY - 1) * map.width + startX].isWalkable))
                return null;
        }
        else
        {
            if (map.tileMap == null || !map.tileMap[startY * map.width + startX].isWalkable || !map.tileMap[endY * map.width + endX].isWalkable)
                return null;
        }


        Tile startTile = map.tileMap[startY * map.width + startX];
        Tile endTile = map.tileMap[endY * map.width + endX];


        openList = new List<Tile> { startTile };
        closedList = new List<Tile>();

        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                Tile tile = map.tileMap[y * map.width + x];
                tile.gCost = int.MaxValue;
                tile.CalcutlateFCost();
                tile.cameFromTile = null;
            }
        }
        startTile.gCost = 0;
        startTile.hCost = CalculateDistanceCost(startTile, endTile);
        startTile.CalcutlateFCost();

        while (openList.Count > 0)
        {
            Tile currentTile = GetLowestFCostTile(openList);
            if (currentTile == endTile)
            {
                return CalculatePath(endTile);
            }
            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach (Tile neighbourTile in GetNeighbourList(currentTile))
            {
                if (closedList.Contains(neighbourTile))
                    continue;
                if (!neighbourTile.isWalkable)
                {
                    closedList.Add(neighbourTile);
                    continue;
                }

                int tentativeGCost = currentTile.gCost + CalculateDistanceCost(currentTile, neighbourTile);
                if (tentativeGCost < neighbourTile.gCost)
                {
                    neighbourTile.cameFromTile = currentTile;
                    neighbourTile.gCost = tentativeGCost;
                    neighbourTile.hCost = CalculateDistanceCost(neighbourTile, endTile);
                    neighbourTile.CalcutlateFCost();

                    if (!openList.Contains(neighbourTile))
                        openList.Add(neighbourTile);
                }
            }
        }
        //out of nodes couldnt find path
        return null;
    }

    private List<Tile> GetNeighbourList(Tile currentTile)
    {
        if (platformer)
        {
            return PlatformerRulesNeighbourList(currentTile);
        }
        else
        {
            return DefaultRuleSet(currentTile);
        }
    }

    Tile GetLowestPlatformBelow(Tile TileToCheck)
    {
        if (map.tileMap[TileToCheck.y * map.width + TileToCheck.x].isWalkable == false)
            return null;
        while (TileToCheck.y - 1 >= 0)
        {
            if (map.tileMap[(TileToCheck.y - 1) * map.width + TileToCheck.x].isWalkable == false)
                return TileToCheck;
            else
            {
                TileToCheck = map.tileMap[(TileToCheck.y - 1) * map.width + TileToCheck.x];
            }
        }
        return null;
    }

    Tile JumpUp1PlatformRight(Tile TileToCheck)
    {
        if (map.tileMap[(TileToCheck.y + 1) * map.width + TileToCheck.x].isWalkable == true &&
            map.tileMap[(TileToCheck.y + 1) * map.width + (TileToCheck.x + 1)].isWalkable == true &&
            map.tileMap[TileToCheck.y * map.width + (TileToCheck.x + 1)].isWalkable == false)
            return map.tileMap[(TileToCheck.y + 1) * map.width + (TileToCheck.x + 1)];
        else
            return null;
    }

    Tile JumpUp1PlatformLeft(Tile TileToCheck)
    {
        if (map.tileMap[(TileToCheck.y + 1) * map.width + TileToCheck.x].isWalkable == true &&
            map.tileMap[(TileToCheck.y + 1) * map.width + (TileToCheck.x - 1)].isWalkable == true &&
            map.tileMap[TileToCheck.y * map.width + (TileToCheck.x - 1)].isWalkable == false)
            return map.tileMap[(TileToCheck.y + 1) * map.width + (TileToCheck.x - 1)];
        else
            return null;
    }

    Tile JumpUp2PlatformRight(Tile TileToCheck)
    {
        if (map.tileMap[(TileToCheck.y + 1) * map.width + TileToCheck.x].isWalkable == true &&
            map.tileMap[(TileToCheck.y + 2) * map.width + TileToCheck.x].isWalkable == true &&
            map.tileMap[(TileToCheck.y + 2) * map.width + (TileToCheck.x + 1)].isWalkable == true &&
            map.tileMap[(TileToCheck.y + 1) * map.width + (TileToCheck.x + 1)].isWalkable == false)
            return map.tileMap[(TileToCheck.y + 2) * map.width + (TileToCheck.x + 1)];
        else
            return null;
    }

    Tile JumpUp2PlatformLeft(Tile TileToCheck)
    {
        if (map.tileMap[(TileToCheck.y + 1) * map.width + TileToCheck.x].isWalkable == true &&
            map.tileMap[(TileToCheck.y + 2) * map.width + TileToCheck.x].isWalkable == true &&
            map.tileMap[(TileToCheck.y + 2) * map.width + (TileToCheck.x - 1)].isWalkable == true &&
            map.tileMap[(TileToCheck.y + 1) * map.width + (TileToCheck.x - 1)].isWalkable == false)
            return map.tileMap[(TileToCheck.y + 2) * map.width + (TileToCheck.x - 1)];
        else
            return null;
    }

    Tile JumpAcrossGapRight(Tile TileToCheck)
    {
        if (map.tileMap[TileToCheck.y * map.width + (TileToCheck.x + 1)].isWalkable == true &&
            map.tileMap[(TileToCheck.y - 1) * map.width + TileToCheck.x + 1].isWalkable == true &&
            map.tileMap[TileToCheck.y * map.width + TileToCheck.x + 2].isWalkable == true)
        {
            Tile tileBelow = GetLowestPlatformBelow(map.tileMap[TileToCheck.y * map.width + TileToCheck.x + 2]);
            if (tileBelow != null)
                return tileBelow;
            else return null;
        }
        else
            return null;
    }

    Tile JumpAcrossGapLeft(Tile TileToCheck)
    {
        if (map.tileMap[TileToCheck.y * map.width + (TileToCheck.x - 1)].isWalkable == true &&
            map.tileMap[(TileToCheck.y - 1) * map.width + TileToCheck.x - 1].isWalkable == true &&
            map.tileMap[TileToCheck.y * map.width + TileToCheck.x - 2].isWalkable == true)
        {
            Tile tileBelow = GetLowestPlatformBelow(map.tileMap[TileToCheck.y * map.width + TileToCheck.x - 2]);
            if (tileBelow != null)
                return tileBelow;
            else return null;
        }
        else
            return null;
    }

    List<Tile> DefaultRuleSet(Tile currentTile)
    {
        List<Tile> neighbourList = new List<Tile>();

        if (currentTile.x - 1 >= 0)
        {
            //Left
            neighbourList.Add(map.tileMap[currentTile.y * map.width + (currentTile.x - 1)]);

            if (map.tileMap[currentTile.y * map.width + (currentTile.x + -1)].isWalkable)
            {
                //Left Down
                if (currentTile.y - 1 >= 0)
                    neighbourList.Add(map.tileMap[(currentTile.y - 1) * map.width + (currentTile.x - 1)]);
                //Left Up
                if (currentTile.y + 1 < map.height)
                    neighbourList.Add(map.tileMap[(currentTile.y + 1) * map.width + (currentTile.x - 1)]);
            }
        }
        if (currentTile.x + 1 < map.width)
        {
            //Right
            neighbourList.Add(map.tileMap[currentTile.y * map.width + (currentTile.x + 1)]);

            if (map.tileMap[currentTile.y * map.width + (currentTile.x + 1)].isWalkable)
            {
                //Right Down
                if (currentTile.y - 1 >= 0 && map.tileMap[currentTile.y * map.width + (currentTile.x + 1)].isWalkable)
                    neighbourList.Add(map.tileMap[(currentTile.y - 1) * map.width + (currentTile.x + 1)]);
                //Right Up
                if (currentTile.y + 1 < map.height && map.tileMap[currentTile.y * map.width + (currentTile.x + 1)].isWalkable)
                    neighbourList.Add(map.tileMap[(currentTile.y + 1) * map.width + (currentTile.x + 1)]);
            }
        }
        //Down
        if (currentTile.y - 1 >= 0)
        {
            neighbourList.Add(map.tileMap[(currentTile.y - 1) * map.width + currentTile.x]);
        }
        //Up
        if (currentTile.y + 1 < map.height)
        {
            neighbourList.Add(map.tileMap[(currentTile.y + 1) * map.width + currentTile.x]);
        }
        return neighbourList;
    }

    List<Tile> PlatformerRulesNeighbourList(Tile currentTile)
    {
        List<Tile> neighbourList = new List<Tile>();
        //left / left down
        if (currentTile.x - 1 >= 0)
        {
            // left and left Down
            Tile LowestPlatform = GetLowestPlatformBelow(map.tileMap[currentTile.y * map.width + (currentTile.x - 1)]);
            if (LowestPlatform != null)
            {
                neighbourList.Add(LowestPlatform);
            }

            //Left Single jump up
            if (currentTile.y + 1 < map.height)
            {
                Tile jump1LeftPlatform = JumpUp1PlatformLeft(currentTile);
                if (jump1LeftPlatform != null)
                {
                    neighbourList.Add(jump1LeftPlatform);
                }
            }
            //Left double jump up
            if (currentTile.y + 2 < map.height)
            {
                Tile jump2LeftPlatform = JumpUp2PlatformLeft(currentTile);
                if (jump2LeftPlatform != null)
                {
                    neighbourList.Add(jump2LeftPlatform);
                }
            }
            //Left jump across
            if (currentTile.x - 2 < map.width & currentTile.y > 0)
            {
                Tile Jump2LeftPlatform = JumpAcrossGapLeft(currentTile);
                if (Jump2LeftPlatform != null)
                    neighbourList.Add(Jump2LeftPlatform);
            }
        }

        //Right movement
        if (currentTile.x + 1 < map.width)
        {
            // Right and Right Down
            Tile LowestPlatform = GetLowestPlatformBelow(map.tileMap[currentTile.y * map.width + (currentTile.x + 1)]);
            if (LowestPlatform != null)
            {
                neighbourList.Add(LowestPlatform);
            }

            //Right Single jump up
            if (currentTile.y + 1 < map.height)
            {
                Tile jump1RightPlatform = JumpUp1PlatformRight(currentTile);
                if (jump1RightPlatform != null)
                {
                    neighbourList.Add(jump1RightPlatform);
                }
            }

            //Right Double jump
            if (currentTile.y + 2 < map.height)
            {
                Tile jump2UpandRightPlatform = JumpUp2PlatformRight(currentTile);
                if (jump2UpandRightPlatform != null)
                {
                    neighbourList.Add(jump2UpandRightPlatform);
                }
            }

            //Right jump across
            if (currentTile.x + 2 < map.width & currentTile.y > 0)
            {
                Tile Jump2RightPlatform = JumpAcrossGapRight(currentTile);
                if (Jump2RightPlatform != null)
                    neighbourList.Add(Jump2RightPlatform);
            }
        }
        return neighbourList;
    }


    private List<Tile> CalculatePath(Tile endTile)
    {
        List<Tile> path = new List<Tile>();
        path.Add(endTile);
        Tile currentTile = endTile;
        while (currentTile.cameFromTile != null)
        {
            path.Add(currentTile.cameFromTile);
            currentTile = currentTile.cameFromTile;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(Tile a, Tile b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return moveDiagonalCost * Mathf.Min(xDistance, yDistance) + moveStraightCost * remaining;
    }

    private Tile GetLowestFCostTile(List<Tile> tileList)
    {
        Tile lowestFCostNode = tileList[0];
        for (int i = 1; i < tileList.Count; i++)
        {
            if (tileList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = tileList[i];
            }
        }
        return lowestFCostNode;
    }
}
