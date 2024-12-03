using System.Collections.Generic;
using UnityEngine;

public class PathCalculator
{
    private PathfindingTile[,] tiles = new PathfindingTile[2000, 2000];

    public void InitialisePathCalculator()
    {
        tiles = new PathfindingTile[2000, 2000];
        
        for (int x = 0; x < 2000; x++)
        {
            for (int y = 0; y < 2000; y++)
            {
                tiles[x, y] = new PathfindingTile();
                tiles[x, y].coord = new Vector2Int(x, y);
                tiles[x, y].isBlocked = false;
            }
        }
    }
    
    
    public void AddBlockedTile(Vector3 pos)
    {
        tiles[(int)(pos.x * 0.5f), (int)(pos.z * 0.5f)].isBlocked = true;
    }
    
    public List<Vector3> GetPath(Vector3 pos1, Vector3 pos2)
    {
        PathfindingTile start = tiles[(int)(pos1.x * 0.5f), (int)(pos1.z * 0.5f)];
        PathfindingTile end = tiles[(int)(pos2.x * 0.5f), (int)(pos2.z * 0.5f)];
        
        List<PathfindingTile> openList = new List<PathfindingTile>();
        List<PathfindingTile> closedList = new List<PathfindingTile>();
        
        openList.Add(start);

        while (openList.Count != 0)
        {
            // We select the best tile of the open list
            int bestDist = 2000;
            int bestIndex = 0;
            for (int i = 0; i < openList.Count; i++)
            {
                int dist = GetManhattanDist(openList[i].coord, end.coord);
                if (dist < bestDist)
                {
                    bestIndex = i;
                    bestDist = dist;
                }
            }

            // We verify if it's the end tile
            PathfindingTile currentTile = openList[bestIndex];
            openList.RemoveAt(bestIndex);
            closedList.Add(currentTile);

            if (currentTile == end)
            {
                return GetFinalPath(start, end);
            }
            
            // Else we add its neighbors to the open list
            List<PathfindingTile> neighborTiles = GetPossibleNeighbors(currentTile);
            for (int i = 0; i < neighborTiles.Count; i++)
            {
                if (!closedList.Contains(neighborTiles[i]) && !openList.Contains(neighborTiles[i]))
                {
                    openList.Add(neighborTiles[i]);
                    neighborTiles[i].previous = currentTile;
                }
            }
        }

        return new List<Vector3>();
    }

    private int GetManhattanDist(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
    }

    private List<Vector3> GetFinalPath(PathfindingTile start, PathfindingTile end)
    {
        List<Vector3> finalPath = new List<Vector3>();
        PathfindingTile currentTile = end;
        
        while (true)
        {
            finalPath.Add(new Vector3(currentTile.coord.x * 2, GenProManager.Instance.currentFloorIndex * 4, currentTile.coord.y * 2));
            if (currentTile == start)
            {
                finalPath.Reverse();
                return finalPath;
            }
            
            currentTile = currentTile.previous;
        }
    }

    private List<PathfindingTile> GetPossibleNeighbors(PathfindingTile tile)
    {
        List<PathfindingTile> availableNeighbors = new List<PathfindingTile>();

        Vector2Int testedPos = tile.coord + Vector2Int.up;
        if (!tiles[testedPos.x, testedPos.y].isBlocked)
        {
            availableNeighbors.Add(tiles[testedPos.x, testedPos.y]);     
        }
        testedPos = tile.coord + Vector2Int.down;
        if (!tiles[testedPos.x, testedPos.y].isBlocked)
        {
            availableNeighbors.Add(tiles[testedPos.x, testedPos.y]);     
        }
        testedPos = tile.coord + Vector2Int.left;
        if (!tiles[testedPos.x, testedPos.y].isBlocked)
        {
            availableNeighbors.Add(tiles[testedPos.x, testedPos.y]);     
        }
        testedPos = tile.coord + Vector2Int.right;
        if (!tiles[testedPos.x, testedPos.y].isBlocked)
        {
            availableNeighbors.Add(tiles[testedPos.x, testedPos.y]);     
        }

        return availableNeighbors;
    }
}
