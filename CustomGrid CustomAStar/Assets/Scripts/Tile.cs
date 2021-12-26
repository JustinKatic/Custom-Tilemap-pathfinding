using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class Tile
{
    public Tile(int x, int y, float worldX, float worldY)
    {
        this.x = x;
        this.y = y;
        this.worldX = worldX;
        this.worldY = worldY;
    }

    public GameObject tilePrefab = null;
    public bool isWalkable = true;


    [HideInInspector] public int x;
    [HideInInspector] public int y;
    [HideInInspector] public float worldX;
    [HideInInspector] public float worldY;


    [HideInInspector] public int currentTileIndex = 0;

    [HideInInspector] public int gCost;
    [HideInInspector] public int hCost;
    [HideInInspector] public int fCost;
    [HideInInspector] public Tile cameFromTile;


    public void CalcutlateFCost()
    {
        fCost = gCost + hCost;
    }
}
