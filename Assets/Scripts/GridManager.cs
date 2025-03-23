using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using static UnityEditor.Progress;
public class GridManager : MonoBehaviour
{
    private static GridManager instance;
    public static GridManager Instance => instance;

    public int width;
    public int height;
    public float cellSize;

    public GameObject redCubePrefab;
    public GameObject greenCubePrefab;
    public GameObject blueCubePrefab;
    public GameObject yellowCubePrefab;
    public GameObject boxPrefab;
    public GameObject hRocketPrefab;
    public GameObject vRocketPrefab;

    private ICellItem[,] gridArray;
    private bool[,] isGridOccupied;
    private int moveCount;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
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
        UIManager.Instance.UpdateUI(moveCount);
    }

    [Serializable]
    private class LevelData
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public string[] grid;
    }

    public void InitializeGrid(string json)
    {
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        width = levelData.grid_width;
        height = levelData.grid_height;
        moveCount = levelData.move_count;



        gridArray = new ICellItem[width, height];
        isGridOccupied = new bool[width, height];

        for (int i = 0; i < levelData.grid.Length; i++)
        {
            int x = i % width;
            int y = i / width;
            CreateItem(x, y, levelData.grid[i]);
        }
    }

    private void CreateItem(int x, int y, string itemCode)
    {
        Vector3 centerPos = GetGridPosition(x, y);
        ICellItem item = null;

        switch (itemCode.ToLower())
        {
            case "r":
                item = Instantiate(redCubePrefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
                item.GameObject.GetComponent<Cube>().cubeColor = CubeColor.Red;
                break;
            case "g":
                item = Instantiate(greenCubePrefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
                item.GameObject.GetComponent<Cube>().cubeColor = CubeColor.Green;
                break;
            case "b":
                item = Instantiate(blueCubePrefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
                item.GameObject.GetComponent<Cube>().cubeColor = CubeColor.Blue;
                break;
            case "y":
                item = Instantiate(yellowCubePrefab, centerPos, Quaternion.identity, transform).AddComponent<Cube>();
                item.GameObject.GetComponent<Cube>().cubeColor = CubeColor.Yellow;
                break;
            case "bo":
                item = Instantiate(boxPrefab, centerPos, Quaternion.identity, transform).AddComponent<Box>();
                break;
            case "hro":
                item = Instantiate(hRocketPrefab, centerPos, Quaternion.identity, transform).AddComponent<Rocket>();
                break;
            case "vro":
                item = Instantiate(vRocketPrefab, centerPos, Quaternion.identity, transform).AddComponent<Rocket>();
                break;
            case "rand":
                item = CreateRandomCube(centerPos);
                break;
            default:
                Debug.LogWarning($"Unknown item code: {itemCode}");
                return;
        }

        if (item != null)
        {

            SetItemAt(x, y, item);
            //item.GridIndex = new Vector2Int(x, y);
            //gridArray[x, y] = item;
            //isGridOccupied[x, y] = true;
            //item.GameObject.name = $"GridItem_{x}_{y}";
        }
    }

    private ICellItem CreateRandomCube(Vector3 centerPos)
    {
        GameObject[] cubePrefabs = { redCubePrefab, greenCubePrefab, blueCubePrefab, yellowCubePrefab };
        CubeColor[] colors = { CubeColor.Red, CubeColor.Green, CubeColor.Blue, CubeColor.Yellow };
        int randomIndex = UnityEngine.Random.Range(0, cubePrefabs.Length);
        ICellItem item = Instantiate(cubePrefabs[randomIndex], centerPos, Quaternion.identity, transform).AddComponent<Cube>();
        item.GameObject.GetComponent<Cube>().cubeColor = colors[randomIndex];
        return item;
    }

    private void CheckClickedPositionIsInTheGrid()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2Int gridIndex = GetGridIndex(mousePosition);

            if (IsInTheGrid(mousePosition))
            {
                ICellItem clickedItem = GetItemAt(gridIndex.x, gridIndex.y);

                if (clickedItem != null)
                {
                    clickedItem.OnTapped();

                    //When Cube Clicked
                    if (clickedItem.ItemType == "Cube")
                    {
                        List<ICellItem> matches = FindMatches(gridIndex);
                        if (matches.Count >= 2)
                        {
                            foreach (ICellItem match in matches)
                            {
                                ClearItemAt(match.GridIndex.x, match.GridIndex.y);


                            }
                            moveCount--;
                        }

                        if (matches.Count >= 4)
                        {
                            CreateItem(gridIndex.x, gridIndex.y, "hro");

                        }


                    }

                    //When Rocket Clicked
                    else if (clickedItem.ItemType == "Rocket")
                    {

                        ////Horizontal Rocket
                        //int Row = gridIndex.y;

                        //for (int x = 0; x < width; x++)
                        //{
                        //    ICellItem item = GetItemAt(x, Row);
                        //    item.TakeDamage(1);
                        //   // ClearItemAt(item.GridIndex.x, Row);
                        //}

                        ////Clear the Rocket
                        //ClearItemAt(gridIndex.x, gridIndex.y);


                        //VerticalRocket
                        int Cloumn = gridIndex.x;

                        for (int y = 0; y < height; y++)
                        {
                            ICellItem item = GetItemAt(Cloumn, y);
                            item.TakeDamage(1);
                            ClearItemAt(Cloumn, item.GridIndex.y);
                        }

                        //Clear the Rocket
                        ClearItemAt(gridIndex.x, gridIndex.y);

                        moveCount--;
                    }
                }

                Fall();
            }
            else
            {
                Debug.Log("Clicked outside the grid.");
            }
        }
    }

    private List<ICellItem> FindMatches(Vector2Int startIndex)
    {
        List<ICellItem> matches = new List<ICellItem>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        CubeColor targetColor = GetItemAt(startIndex.x, startIndex.y)?.GameObject.GetComponent<Cube>().cubeColor ?? CubeColor.Red;

        void CheckCell(int x, int y)
        {
            if (!IsValidPosition(x, y) || visited.Contains(new Vector2Int(x, y))) return;
            ICellItem item = GetItemAt(x, y);
            if (item != null && item.ItemType == "Cube" && item.GameObject.GetComponent<Cube>().cubeColor == targetColor)
            {
                matches.Add(item);
                visited.Add(new Vector2Int(x, y));
                CheckCell(x + 1, y); CheckCell(x - 1, y); CheckCell(x, y + 1); CheckCell(x, y - 1);
            }
        }

        CheckCell(startIndex.x, startIndex.y);
        return matches;
    }

    private void Fall()
    {
        float duration = 0.5f; // Animation duration in seconds

        // Step 1: Move existing items down
        for (int x = 0; x < width; x++)
        {
            int bottomEmptyY = 0;
            for (int y = 0; y < height; y++)
            {
                ICellItem item = GetItemAt(x, y);
                if (item == null)
                {
                    continue;
                }
                else if (item.CanFall() && y > bottomEmptyY)
                {

                    Vector3 targetPos = GetGridPosition(x, bottomEmptyY);
                    // Animate movement with DOTween
                    item.GameObject.transform.DOMove(targetPos, duration).SetEase(Ease.OutQuad);
                    // Update grid immediately

                    gridArray[x, bottomEmptyY] = item;
                    gridArray[x, y] = null;
                    isGridOccupied[x, bottomEmptyY] = true;
                    isGridOccupied[x, y] = false;
                    item.GridIndex = new Vector2Int(x, bottomEmptyY);
                    bottomEmptyY++;
                }
                else
                {
                    bottomEmptyY = y + 1;
                }
            }

            // Step 2: Spawn new cubes from the top
            for (int y = bottomEmptyY; y < height; y++)
            {
                if (!isGridOccupied[x, y])
                {
                    Vector3 spawnPos = GetGridPosition(x, y);
                    ICellItem newCube = CreateRandomCube(spawnPos);
                    // Start slightly above the grid for a falling effect
                    Vector3 startPos = spawnPos + Vector3.up * cellSize;

                    newCube.GameObject.transform.position = startPos;
                    newCube.GameObject.transform.DOMove(spawnPos, duration).SetEase(Ease.OutQuad);
                    // Update grid
                    gridArray[x, y] = newCube;
                    isGridOccupied[x, y] = true;
                    newCube.GridIndex = new Vector2Int(x, y);
                }
            }
        }
    }

    public void SetItemAt(int x, int y, ICellItem item)
    {
        if (IsValidPosition(x, y))
        {
            gridArray[x, y] = item;
            isGridOccupied[x, y] = (item != null);
            if (item != null) { item.GridIndex = new Vector2Int(x, y); item.GameObject.transform.position = GetGridPosition(x, y); item.GameObject.name = $"GridItem_{x}_{y}"; }
        }
    }
    public void ClearItemAt(int x, int y)
    {
        if (IsValidPosition(x, y) && gridArray[x, y] != null)
        {
            Destroy(gridArray[x, y].GameObject); // Fully destroy the GameObject
            gridArray[x, y] = null;
            isGridOccupied[x, y] = false;
        }
    }
    // Utility methods
    public Vector3 GetNearestGridPos(Vector3 worldPosition) => GetGridPosition(GetGridIndex(worldPosition).x, GetGridIndex(worldPosition).y);
    private void OccupyGrid(Vector2Int gridIndex) { if (IsValidPosition(gridIndex.x, gridIndex.y)) isGridOccupied[gridIndex.x, gridIndex.y] = true; }
    public void UnOccupyGrid(Vector3 position) { Vector2Int gridIndex = GetGridIndex(position); if (IsValidPosition(gridIndex.x, gridIndex.y)) isGridOccupied[gridIndex.x, gridIndex.y] = false; }
    public bool IsGridAvailable(Vector3 position) { Vector2Int gridIndex = GetGridIndex(position); return IsInTheGrid(position) && !isGridOccupied[gridIndex.x, gridIndex.y]; }
    public bool IsInTheGrid(Vector3 position) { Vector2Int gridIndex = GetGridIndex(position); return IsValidPosition(gridIndex.x, gridIndex.y); }
    public ICellItem GetItemAt(int x, int y) => IsValidPosition(x, y) ? gridArray[x, y] : null;
    private bool IsValidPosition(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
    public Vector2Int GetGridIndex(Vector3 worldPosition) => new Vector2Int(Mathf.FloorToInt((worldPosition - transform.position).x / cellSize), Mathf.FloorToInt((worldPosition - transform.position).y / cellSize));
    public Vector3 GetGridPosition(int x, int y) => transform.position + new Vector3(x * cellSize, y * cellSize, 0);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i <= width; i++) { Vector3 start = transform.position + new Vector3(i * cellSize, 0, 0); Vector3 end = transform.position + new Vector3(i * cellSize, height * cellSize, 0); Gizmos.DrawLine(start, end); }
        for (int j = 0; j <= height; j++) { Vector3 start = transform.position + new Vector3(0, j * cellSize, 0); Vector3 end = transform.position + new Vector3(width * cellSize, j * cellSize, 0); Gizmos.DrawLine(start, end); }
    }
}