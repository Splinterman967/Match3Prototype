using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridManager : MonoBehaviour
{
    // Public variables to configure the grid in the Inspector
    public int width;          // Number of columns in the grid (overridden by JSON)
    public int height;         // Number of rows in the grid (overridden by JSON)
    public float cellSize;     // Size of each grid cell (width and height)

    // Prefabs for each item type
    public GameObject redCubePrefab;
    public GameObject greenCubePrefab;
    public GameObject blueCubePrefab;
    public GameObject yellowCubePrefab;
    public GameObject boxPrefab;
    // Add more prefabs for rockets, stones, vases, etc., as needed

    // Private grid data
    private ICellItem[,] gridArray; // 2D array to store grid items
    private bool[,] isGridOccupied; // Tracks occupied cells
    private int moveCount;          // Move count for the level

    void Start()
    {
        // Example JSON string (replace with file loading later)
        string json = @"{
            ""level_number"": 1,
            ""grid_width"": 9,
            ""grid_height"": 10,
            ""move_count"": 20,
            ""grid"": [""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""bo"", ""r"", ""r"", ""r"", ""r"", ""g"", ""b"", ""b"", ""b"", ""b"", ""y"", ""y"", ""y"", ""y"", ""g"", ""y"", ""y"", ""y"", ""y"", ""b"", ""b"", ""b"", ""b"", ""y"", ""r"", ""r"", ""r"", ""r"", ""rand"", ""rand"", ""rand"", ""rand"", ""y"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand""]
        }";
        InitializeGrid(json);
    }

    void Update()
    {
        CheckClickedPositionIsInTheGrid();
    }

    // Level data class to deserialize JSON
    [Serializable]
    private class LevelData
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public string[] grid;
    }

    // Function to initialize the grid from JSON
    public void InitializeGrid(string json)
    {
        // Parse JSON into LevelData
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        width = levelData.grid_width;
        height = levelData.grid_height;
        moveCount = levelData.move_count;

        // Initialize arrays
        gridArray = new ICellItem[width, height];
        isGridOccupied = new bool[width, height];

        // Populate the grid from the grid array
        for (int i = 0; i < levelData.grid.Length; i++)
        {
            int x = i % width;    // Column (horizontal position)
            int y = i / width;    // Row (vertical position)
            CreateItem(x, y, levelData.grid[i]);
        }
    }

    // Create an item at the specified grid position based on the item code
    private void CreateItem(int x, int y, string itemCode)
    {
        Vector3 centerPos = GetGridPosition(x, y);
        ICellItem item = null;

        switch (itemCode.ToLower())
        {
            case "r": // Red Cube
                item = Instantiate(redCubePrefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
                break;
            case "g": // Green Cube
                item = Instantiate(greenCubePrefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
                break;
            case "b": // Blue Cube
                item = Instantiate(blueCubePrefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
                break;
            case "y": // Yellow Cube
                item = Instantiate(yellowCubePrefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
                break;
            case "bo": // Box Obstacle
                item = Instantiate(boxPrefab, centerPos, Quaternion.identity, transform).AddComponent<Box>();
                break;
            case "rand": // Random Cube
                item = CreateRandomCube(centerPos);
                break;
            default:
                Debug.LogWarning($"Unknown item code: {itemCode}");
                return;
        }

        if (item != null)
        {
            item.GridPosition = new Vector2Int(x, y);
            gridArray[x, y] = item;
            isGridOccupied[x, y] = true;
            item.GameObject.name = $"GridItem_{x}_{y}";
        }
    }

    // Create a random cube (r, g, b, y)
    private ICellItem CreateRandomCube(Vector3 centerPos)
    {
        GameObject[] cubePrefabs = { redCubePrefab, greenCubePrefab, blueCubePrefab, yellowCubePrefab };
        GameObject prefab = cubePrefabs[UnityEngine.Random.Range(0, cubePrefabs.Length)];
        return Instantiate(prefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
    }

    // Get the nearest grid position for a world position
    public Vector3 GetNearestGridPos(Vector3 worldPosition)
    {
        Vector2Int gridIndex = GetGridIndex(worldPosition);
        Vector3 gridPos = GetGridPosition(gridIndex.x, gridIndex.y);
        if (IsInTheGrid(worldPosition))
            OccupyGrid(gridIndex);
        return gridPos;
    }

    // Occupy a grid cell
    private void OccupyGrid(Vector2Int gridIndex)
    {
        if (IsValidPosition(gridIndex.x, gridIndex.y))
            isGridOccupied[gridIndex.x, gridIndex.y] = true;
    }

    // Unoccupy a grid cell
    public void UnOccupyGrid(Vector3 position)
    {
        Vector2Int gridIndex = GetGridIndex(position);
        if (IsValidPosition(gridIndex.x, gridIndex.y))
            isGridOccupied[gridIndex.x, gridIndex.y] = false;
    }

    // Check if a grid cell is available
    public bool IsGridAvailable(Vector3 position)
    {
        Vector2Int gridIndex = GetGridIndex(position);
        if (IsInTheGrid(position))
            return !isGridOccupied[gridIndex.x, gridIndex.y];
        return false;
    }

    // Check if a position is within the grid bounds
    public bool IsInTheGrid(Vector3 position)
    {
        Vector2Int gridIndex = GetGridIndex(position);
        return IsValidPosition(gridIndex.x, gridIndex.y);
    }

    // Get item at grid position
    public ICellItem GetItemAt(int x, int y)
    {
        if (IsValidPosition(x, y))
            return gridArray[x, y];
        return null;
    }

    // Set item at grid position
    public void SetItemAt(int x, int y, ICellItem item)
    {
        if (IsValidPosition(x, y))
        {
            gridArray[x, y] = item;
            isGridOccupied[x, y] = (item != null);
            if (item != null)
            {
                item.GridPosition = new Vector2Int(x, y);
                item.GameObject.transform.position = GetGridPosition(x, y);
                item.GameObject.name = $"GridItem_{x}_{y}";
            }
        }
    }

    // Clear an item from the grid
    public void ClearItemAt(int x, int y)
    {
        if (IsValidPosition(x, y) && gridArray[x, y] != null)
        {
            gridArray[x, y].DestroyItem();
            gridArray[x, y] = null;
            isGridOccupied[x, y] = false;
        }
    }

    // Check if the position is within grid bounds
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // Function to calculate the grid index (x, y) from a world position
    public Vector2Int GetGridIndex(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;
        int x = Mathf.FloorToInt(localPosition.x / cellSize);
        int y = Mathf.FloorToInt(localPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    // Function to calculate the world position of a grid cell's center
    public Vector3 GetGridPosition(int x, int y)
    {
        return transform.position + new Vector3(x * cellSize, y * cellSize, 0);
    }

    // Draw grid lines in the Scene View using Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i <= width; i++)
        {
            Vector3 start = transform.position + new Vector3(i * cellSize, 0, 0);
            Vector3 end = transform.position + new Vector3(i * cellSize, height * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
        for (int j = 0; j <= height; j++)
        {
            Vector3 start = transform.position + new Vector3(0, j * cellSize, 0);
            Vector3 end = transform.position + new Vector3(width * cellSize, j * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    // Check if a grid position was clicked
    private void CheckClickedPositionIsInTheGrid()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int gridIndex = GetGridIndex(mousePosition);

            if (IsInTheGrid(mousePosition))
            {
                //Debug.Log($"Clicked Grid Index: ({gridIndex.x}, {gridIndex.y})");
               // Debug.Log($"Clicked Grid Position: {GetGridPosition(gridIndex.x, gridIndex.y)})");
                ICellItem item = GetItemAt(gridIndex.x, gridIndex.y);
                if (item != null)
                    item.OnTapped();
            }
            else
            {
                Debug.Log("Clicked outside the grid.");
            }
        }
    }
}