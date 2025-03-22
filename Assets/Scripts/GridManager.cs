using UnityEngine;
using System;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 6;    // Default width (overridden by JSON)
    [SerializeField] private int gridHeight = 6;   // Default height (overridden by JSON)
    [SerializeField] private float cellSize = 1f;  // Size of each cell in world units
    [SerializeField] private Vector2 gridOffset;   // Offset to center the grid in the scene

    // Prefabs for each item type
    [SerializeField] private GameObject redCubePrefab;
    [SerializeField] private GameObject greenCubePrefab;
    [SerializeField] private GameObject blueCubePrefab;
    [SerializeField] private GameObject yellowCubePrefab;
    [SerializeField] private GameObject boxPrefab;
    // Add more prefabs for rockets, stones, vases, etc., as needed

    private ICellItem[,] grid; // 2D array to store items
    private int moveCount;     // Move count for the level

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
        InitializeGridFromJson(json);
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

    // Initialize the grid based on JSON data
    private void InitializeGridFromJson(string json)
    {
        // Parse JSON into LevelData
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        gridWidth = levelData.grid_width;
        gridHeight = levelData.grid_height;
        moveCount = levelData.move_count;
        grid = new ICellItem[gridWidth, gridHeight];

        // Populate the grid from the grid array
        for (int i = 0; i < levelData.grid.Length; i++)
        {
            int x = i % gridWidth;           // Column (horizontal position)
            int y = i / gridWidth;           // Row (vertical position)
            CreateItem(x, y, levelData.grid[i]);
        }
    }

    // Create an item at the specified grid position based on the item code
    private void CreateItem(int x, int y, string itemCode)
    {
        Vector3 worldPos = GridToWorldPosition(x, y);
        ICellItem item = null;

        switch (itemCode.ToLower())
        {
            case "r": // Red Cube
                item = Instantiate(redCubePrefab, worldPos, Quaternion.identity, transform).AddComponent<Cube>();
                break;
            case "g": // Green Cube
                item = Instantiate(greenCubePrefab, worldPos, Quaternion.identity, transform).AddComponent<Cube>();
                break;
            case "b": // Blue Cube
                item = Instantiate(blueCubePrefab, worldPos, Quaternion.identity, transform).AddComponent<Cube>();
                break;
            case "y": // Yellow Cube
                item = Instantiate(yellowCubePrefab, worldPos, Quaternion.identity, transform).AddComponent<Cube>();
                break;
            case "bo": // Box Obstacle
                item = Instantiate(boxPrefab, worldPos, Quaternion.identity, transform).AddComponent<Box>();
                break;
            case "rand": // Random Cube
                item = CreateRandomCube(worldPos);
                break;
            // Add cases for "vro" (vertical rocket), "hro" (horizontal rocket), "s" (stone), "v" (vase) later
            default:
                Debug.LogWarning($"Unknown item code: {itemCode}");
                return;
        }

        if (item != null)
        {
            item.GridPosition = new Vector2Int(x, y);
            grid[x, y] = item;
        }
    }

    // Create a random cube (r, g, b, y)
    private ICellItem CreateRandomCube(Vector3 worldPos)
    {
        GameObject[] cubePrefabs = { redCubePrefab, greenCubePrefab, blueCubePrefab, yellowCubePrefab };
        GameObject prefab = cubePrefabs[UnityEngine.Random.Range(0, cubePrefabs.Length)];
        return Instantiate(prefab, worldPos, Quaternion.identity, transform).AddComponent<Cube>();
    }

    // Convert grid coordinates to world position
    private Vector3 GridToWorldPosition(int x, int y)
    {
        float worldX = x * cellSize + gridOffset.x;
        float worldY = y * cellSize + gridOffset.y;
        return new Vector3(worldX, worldY, 0f);
    }

    // Get item at grid position
    public ICellItem GetItemAt(int x, int y)
    {
        if (IsValidPosition(x, y))
            return grid[x, y];
        return null;
    }

    // Set item at grid position
    public void SetItemAt(int x, int y, ICellItem item)
    {
        if (IsValidPosition(x, y))
        {
            grid[x, y] = item;
            if (item != null)
            {
                item.GridPosition = new Vector2Int(x, y);
                item.GameObject.transform.position = GridToWorldPosition(x, y);
            }
        }
    }

    // Check if the position is within grid bounds
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    // Clear an item from the grid
    public void ClearItemAt(int x, int y)
    {
        if (IsValidPosition(x, y) && grid[x, y] != null)
        {
            grid[x, y].DestroyItem();
            grid[x, y] = null;
        }
    }

    // For debugging: Visualize grid in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = GridToWorldPosition(x, y);
                Gizmos.DrawWireCube(pos, new Vector3(cellSize, cellSize, 0f));
            }
        }
    }
}