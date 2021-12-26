using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class Map : MonoBehaviour
{
    [HideInInspector] public Tile[] tileMap;

    [SerializeField] PathFinding pathFinding;

    [SerializeField] List<Tile> tileList = new List<Tile>();
    [SerializeField] Transform parent;

    public int NewWidth;
    public int NewHeight;
    public float NewCellSize = 1;

    [HideInInspector] public int width;
    [HideInInspector] public int height;
    [HideInInspector] public float cellSize = 1;


    [HideInInspector] public bool shouldRenderPossiblePaths = false;
    [HideInInspector] public bool shouldRenderGrid = false;
    [HideInInspector] public bool shouldRenderPathFindingDebugTools = false;


    [SerializeField] private UnityEvent TileChangedEvent;

    private List<Vector3> path;
    private Vector3Int startPos;
    private Vector3Int endPos;

    List<List<Vector3>> possiblePathsContainer;


    //Creates new map and populates elements with Tile ref
    public void CreateMap()
    {
        if (tileMap != null)
            DestoryAllPrefabs();

        width = NewWidth;
        height = NewHeight;
        cellSize = NewCellSize;

        tileMap = new Tile[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tileMap[y * width + x] = new Tile(x, y, GetWorldPos(x, y).x, GetWorldPos(x, y).y);
            }
        }
    }

    //Removes all prefabs and sets Tiles back to default ref.
    public void ResetMap()
    {
        if (tileMap == null)
            return;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tileMap[y * width + x].tilePrefab != null)
                {
                    DestroyImmediate(tileMap[y * width + x].tilePrefab.gameObject, true);
                }
                tileMap[y * width + x] = new Tile(x, y, GetWorldPos(x, y).x, GetWorldPos(x, y).y);
            }
        }
        SetPath();
    }

    //Updates tile list indexs if a new tile is added to list
    private void OnValidate()
    {
        for (int i = 0; i < tileList.Count; i++)
        {
            tileList[i].currentTileIndex = i;
        }
    }


    //Destroys all prefabs tiles in scene called when creating or loading in new map
    void DestoryAllPrefabs()
    {
        if (tileMap == null)
            return;
        if (tileMap.Length <= 0)
            return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tileMap[y * width + x] != null)
                {
                    if (tileMap[y * width + x].tilePrefab != null)
                    {
                        DestroyImmediate(tileMap[y * width + x].tilePrefab.gameObject, true);
                    }
                }
            }
        }
    }

    //Loads all prefabs 
    void LoadPrefabs()
    {
        if (tileMap == null)
            return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = tileMap[y * width + x];
                if (tile.currentTileIndex != 0)
                {
                    tile.tilePrefab = Instantiate(tileList[tile.currentTileIndex].tilePrefab, new Vector2(tile.worldX + (cellSize * 0.5f), tile.worldY + (cellSize * 0.5f)), Quaternion.identity, parent);
                }
            }
        }
    }


    //Gets next tile from list of tiles
    void GetNextTileIndex(Tile Tile)
    {
        if (Tile.currentTileIndex + 1 >= tileList.Count)
        {
            Tile.currentTileIndex = 0;
        }
        else
        {
            Tile.currentTileIndex++;
        }
    }

    //Gets world pos of the tiles origin
    public Vector3 GetWorldPos(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

    //Returns the X and Y rounded to int of a world position
    public Vector2Int GetXY(Vector3 worldPos)
    {
        Vector2Int XY = new Vector2Int();
        XY.x = Mathf.FloorToInt(worldPos.x / cellSize);
        XY.y = Mathf.FloorToInt(worldPos.y / cellSize);
        return XY;
    }


    //Sets value of tile to next tile in list
    public void SetValue(Tile tile)
    {
        if (tile.tilePrefab != null)
        {
            DestroyImmediate(tile.tilePrefab.gameObject, true);
            tile.tilePrefab = null;
        }
        if (tile.currentTileIndex != 0)
        {
            tile.tilePrefab = Instantiate(tileList[tile.currentTileIndex].tilePrefab, new Vector2(tile.worldX + (cellSize * 0.5f), tile.worldY + (cellSize * 0.5f)), Quaternion.identity, parent);
        }

        tile.isWalkable = tileList[tile.currentTileIndex].isWalkable;

        SetPath();
        TileChangedEvent.Invoke();
    }

    //Sets value at tile X Y when given a world position
    public void SetValue(Vector3 worldPos)
    {
        int x = GetXY(worldPos).x;
        int y = GetXY(worldPos).y;

        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            GetNextTileIndex(tileMap[y * width + x]);
            SetValue(tileMap[y * width + x]);
        }
    }


    //Populate pathfinding path
    public void SetPath()
    {
        if (endPos != null)
        {
            Vector3 startWorldPos = GetWorldPos(startPos.x, startPos.y);
            Vector3 endWorldPos = GetWorldPos(endPos.x, endPos.y);

            path = pathFinding.FindPath(startWorldPos, endWorldPos);
        }
    }


    //Sets the value pathfinding end pos in grid space
    public void SetEndPos(Vector3 worldPos)
    {
        int x = GetXY(worldPos).x;
        int y = GetXY(worldPos).y;
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            endPos.x = x;
            endPos.y = y;
        }
        SetPath();
    }

    //Sets the value pathfinding start pos in grid space
    public void SetStartPos(Vector3 worldPos)
    {
        int x = GetXY(worldPos).x;
        int y = GetXY(worldPos).y;
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            startPos.x = x;
            startPos.y = y;
        }
        if (endPos != null)
            SetPath();
    }


    public void GetAllPossiblePaths()
    {
        possiblePathsContainer = new List<List<Vector3>>();
        foreach (var tile in tileMap)
        {
            foreach (var tile2 in tileMap)
            {
                var path = pathFinding.FindPath(new Vector2(tile.worldX, tile.worldY), new Vector2(tile2.worldX, tile2.worldY));

                if (path == null || path.Count <= 1)
                    continue;

                List<Vector3> possiblePath = new List<Vector3>();
                foreach (var p in path)
                {
                    possiblePath.Add(p);
                }
                possiblePathsContainer.Add(possiblePath);
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Draws grid map
        Gizmos.color = Color.green;
        if (shouldRenderGrid)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.DrawLine(GetWorldPos(x, y), GetWorldPos(x, y + 1));
                    Gizmos.DrawLine(GetWorldPos(x, y), GetWorldPos(x + 1, y));
                }
            }
            Gizmos.DrawLine(GetWorldPos(0, height), GetWorldPos(width, height));
            Gizmos.DrawLine(GetWorldPos(width, 0), GetWorldPos(width, height));
        }

        if (shouldRenderPathFindingDebugTools)
        {
            Gizmos.color = Color.magenta;
            //Draws pathfinding path
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawLine(new Vector3(path[i].x, path[i].y), new Vector3(path[i + 1].x, path[i + 1].y));
                }
            }

            //Draws path finding start pos location
            Gizmos.color = Color.green;
            if (startPos != null)
            {
                Vector3 startWorldPos = GetWorldPos(startPos.x, startPos.y);
                Gizmos.DrawSphere(new Vector3(startWorldPos.x + cellSize * 0.5f, startWorldPos.y + cellSize * 0.5f), cellSize / 2);
            }
            //Draws path finding end pos location
            Gizmos.color = Color.red;
            if (endPos != null)
            {
                Vector3 endWorldPos = GetWorldPos(endPos.x, endPos.y);
                Gizmos.DrawCube(new Vector3(endWorldPos.x + cellSize * 0.5f, endWorldPos.y + cellSize * 0.5f), new Vector3(cellSize * 0.5f, cellSize * 0.5f));
            }
        }


        if (shouldRenderPossiblePaths && possiblePathsContainer != null)
        {
            foreach (var path in possiblePathsContainer)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawLine(new Vector3(path[i].x, path[i].y), new Vector3(path[i + 1].x, path[i + 1].y));
                }
            }
        }
    }


    //called when a tile is changed
    public void DebugTest()
    {
        Debug.Log("Tile Changed");
    }

    //Saves to Json
    public void Save(string fileNameToSave)
    {
        SaveMapObj saveObj = new SaveMapObj
        {
            tileMap = this.tileMap,
            width = this.width,
            height = this.height,
            cellSize = this.cellSize
        };
        string json = JsonUtility.ToJson(saveObj);
        File.WriteAllText(Application.dataPath + fileNameToSave, json);
    }

    //Loads from Json
    public void Load(string fileToLoad)
    {
        if (File.Exists(Application.dataPath + fileToLoad))
        {
            string saveString = File.ReadAllText(Application.dataPath + fileToLoad);
            SaveMapObj saveObject = JsonUtility.FromJson<SaveMapObj>(saveString);

            DestoryAllPrefabs();
            tileMap = saveObject.tileMap;
            width = saveObject.width;
            height = saveObject.height;
            cellSize = saveObject.cellSize;
            LoadPrefabs();
        }
        SetPath();
    }

    //Class to save and load
    private class SaveMapObj
    {
        public Tile[] tileMap;

        public int width;
        public int height;
        public float cellSize;
    }
}
