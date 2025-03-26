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

    [SerializeField] private GameObject redCubePrefab;
    [SerializeField] private GameObject greenCubePrefab;
    [SerializeField] private GameObject blueCubePrefab;
    [SerializeField] private GameObject yellowCubePrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private GameObject vasePrefab;
    [SerializeField] private GameObject hRocketPrefab;
    [SerializeField] private GameObject vRocketPrefab;

    public ICellItem[,] gridArray;
    public int moveCount;
    public int ObstacleCount;
    private bool[,] isGridOccupied;

    //For reading the level Json
    [Serializable]
    public class LevelData
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public string[] grid;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        CheckInput();

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
        UIManager.Instance.UpdateMoveCountUI(moveCount);
        GameManager.Instance.CheckWinCondition();
    }

    private void CreateItem(int x, int y, string itemCode)
    {
        Vector3 centerPos = GetGridPosition(x, y);
        ICellItem item = null;

        switch (itemCode.ToLower())
        {
            case "r":
                item = Instantiate(redCubePrefab, centerPos, Quaternion.identity, transform).GetComponent<Cube>();
                break;
            case "g":
                item = Instantiate(greenCubePrefab, centerPos, Quaternion.identity, transform).GetComponent<Cube>();
                break;
            case "b":
                item = Instantiate(blueCubePrefab, centerPos, Quaternion.identity, transform).GetComponent<Cube>();
                break;
            case "y":
                item = Instantiate(yellowCubePrefab, centerPos, Quaternion.identity, transform).GetComponent<Cube>();
                break;
            case "bo":
                item = Instantiate(boxPrefab, centerPos, Quaternion.identity, transform).GetComponent<Box>();
                break;
            case "s":
                item = Instantiate(stonePrefab, centerPos, Quaternion.identity, transform).GetComponent<Stone>();
                break;
            case "v":
                item = Instantiate(vasePrefab, centerPos, Quaternion.identity, transform).GetComponent<Vase>();
                break;
            case "hro":
                item = Instantiate(hRocketPrefab, centerPos, Quaternion.identity, transform).GetComponent<Rocket>();
                break;
            case "vro":
                item = Instantiate(vRocketPrefab, centerPos, Quaternion.identity, transform).GetComponent<Rocket>();
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
        }
    }

    private ICellItem CreateRandomCube(Vector3 centerPos)
    {
        GameObject[] cubePrefabs = { redCubePrefab, greenCubePrefab, blueCubePrefab, yellowCubePrefab };
        ItemCode[] colors = { ItemCode.r, ItemCode.g, ItemCode.b, ItemCode.y };

        int randomIndex = UnityEngine.Random.Range(0, cubePrefabs.Length);

        ICellItem item = Instantiate(cubePrefabs[randomIndex], centerPos, Quaternion.identity, transform).AddComponent<Cube>();
        item.GameObject.GetComponent<Cube>().ItemCode = colors[randomIndex];
        return item;
    }

    private float inputCooldown = 0.2f;
    private bool isInputAllowed = true;

    private void CheckInput()
    {
        if (!isInputAllowed || !Input.GetMouseButtonDown(0)) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridIndex = GetGridIndex(mousePosition);

        if (!IsInTheGrid(mousePosition))
        {
            Debug.Log("Clicked outside the grid.");
            return;
        }

        StartCoroutine(HandleInputWithCooldown(gridIndex));
    }

    private IEnumerator HandleInputWithCooldown(Vector2Int gridIndex)
    {
        isInputAllowed = false;

        ICellItem clickedItem = GetItemAt(gridIndex.x, gridIndex.y);
        if (clickedItem != null)
        {
            yield return HandleItemClick(clickedItem, gridIndex);
        }

        yield return new WaitForSeconds(inputCooldown);
        isInputAllowed = true;
    }

    private IEnumerator HandleItemClick(ICellItem clickedItem, Vector2Int gridIndex)
    {
        clickedItem.OnTapped();

        switch (clickedItem)
        {
            case Cube _:
                yield return HandleCubeClick(gridIndex, (Cube)clickedItem);
                break;

            case Rocket rocket:
                yield return HandleRocketClick(gridIndex, rocket);
                break;
            default:
                Debug.Log($"No special handling for {clickedItem.GetType()}");
                break;
        }

        // Update game state after any interaction
        yield return StartCoroutine(Fall(() => {
            UIManager.Instance.UpdateMoveCountUI(moveCount);
            GameManager.Instance.CheckWinCondition();
        }));
    }

    private IEnumerator HandleCubeClick(Vector2Int gridIndex, Cube clickedCube)
    {
        List<ICellItem> matches = FindMatches(gridIndex);

        if (matches.Count >= 2)
        {
            // First damage adjacent obstacles from each matched cube
            foreach (ICellItem match in matches)
            {
                if (match is Cube cube)
                {
                    // Damage adjacent obstacles
                    ICellItem[] neighbors = cube.GetNeighbours();
                    foreach (ICellItem neighbor in neighbors)
                    {
                        if (neighbor != null &&
                            (neighbor.ItemCode == ItemCode.bo || neighbor.ItemCode == ItemCode.v))
                        {
                            neighbor.TakeDamage(1);
                        }
                    }
                }
            }

            // Then clear all matched items
            foreach (ICellItem match in matches)
            {
                ClearItemAt(match.GridIndex.x, match.GridIndex.y, false);
            }

            // Wait one frame to ensure grid is cleared
            yield return null;

            // Then check for special item creation
            if (matches.Count >= 4 && gridArray[gridIndex.x, gridIndex.y] == null)
            {
                CreateRocketItem(gridIndex, matches.Count);
            }

            moveCount--;

            // Reset damage tracking for vases after blast
            foreach (ICellItem item in gridArray)
            {
                if (item is Vase vase)
                {
                    vase.ResetDamageTracking();
                }
            }

            yield return StartCoroutine(Fall(() => {
                UIManager.Instance.UpdateMoveCountUI(moveCount);
                GameManager.Instance.CheckWinCondition();
            }));
        }
    }

    private IEnumerator HandleRocketClick(Vector2Int gridIndex, Rocket rocket)
    {
        rocket.OnTapped(); 
        moveCount--;

        // Wait for rocket animation to complete
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(Fall(() => {
            UIManager.Instance.UpdateMoveCountUI(moveCount);
            GameManager.Instance.CheckWinCondition();
        }));
    }

   private void CreateRocketItem(Vector2Int position, int matchCount)
{
    if (!IsValidPosition(position.x, position.y) || gridArray[position.x, position.y] != null)
        return;

    string specialItemCode = null;

    if (matchCount >= 4)
    {
        specialItemCode = UnityEngine.Random.Range(0, 2) == 0 ? "hro" : "vro";
        CreateItem(position.x, position.y, specialItemCode);
        
        // Animate the new rocket
        ICellItem newRocket = GetItemAt(position.x, position.y);
        if (newRocket != null)
        {
            newRocket.GameObject.transform.localScale = Vector3.zero;
            newRocket.GameObject.transform
                .DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack);
        }
    }
}
    private List<ICellItem> FindMatches(Vector2Int startIndex)
    {
        List<ICellItem> matches = new List<ICellItem>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        ICellItem startItem = GetItemAt(startIndex.x, startIndex.y);
        if (startItem?.ItemType != "Cube") return matches;

        ItemCode targetColor = startItem.GameObject.GetComponent<Cube>().ItemCode;
        queue.Enqueue(startIndex);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (!IsValidPosition(current.x, current.y) || visited.Contains(current)) continue;

            ICellItem item = GetItemAt(current.x, current.y);
            if (item?.ItemType == "Cube" && item.GameObject.GetComponent<Cube>().ItemCode == targetColor)
            {
                matches.Add(item);
                visited.Add(current);

                // Enqueue neighbors
                queue.Enqueue(new Vector2Int(current.x + 1, current.y));
                queue.Enqueue(new Vector2Int(current.x - 1, current.y));
                queue.Enqueue(new Vector2Int(current.x, current.y + 1));
                queue.Enqueue(new Vector2Int(current.x, current.y - 1));
            }
        }
        return matches;
    }
    public IEnumerator Fall(Action onComplete)
    {
        float duration = 0.5f; // Animation duration in seconds

        List<Tween> activeTweens = new List<Tween>();

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
   
                    item.GameObject.transform.DOMove(targetPos, duration).SetEase(Ease.OutQuad);
            

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

        foreach (Tween tween in activeTweens)
        {
            yield return tween.WaitForCompletion();
        }

        onComplete?.Invoke();
    }

    public void SetItemAt(int x, int y, ICellItem item)
    {
        if (IsValidPosition(x, y))
        {
            gridArray[x, y] = item;
            isGridOccupied[x, y] = (item != null);
            if (item != null) { item.GridIndex = new Vector2Int(x, y); item.GameObject.transform.position = GetGridPosition(x, y); }

            // item.GameObject.name = $"GridItem_{x}_{y}"
        }
    }
    public void ClearItemAt(int x, int y, bool immediateFall = true)
    {
        if (!IsValidPosition(x, y)) return;

        ICellItem item = gridArray[x, y];
        if (item != null && item.ItemType != "Rocket") // Skip particle effects for rockets
        {
            if (item.DestructionParticles != null)
            {
                GameObject particles = Instantiate(
                    item.DestructionParticles,
                    GetGridPosition(x, y),
                    Quaternion.identity
                );

                ParticleSystem[] allParticles = particles.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in allParticles)
                {
                    ps.Play();
                }

                ParticleSystem mainParticle = particles.GetComponent<ParticleSystem>();
                float duration = mainParticle != null ? mainParticle.main.duration : 1f;
                Destroy(particles, duration);
            }

            Destroy(item.GameObject);
        }

        gridArray[x, y] = null;
        isGridOccupied[x, y] = false;

        if (immediateFall)
        {
            StartCoroutine(Fall(() => {
                UIManager.Instance.UpdateMoveCountUI(moveCount);
                GameManager.Instance.CheckWinCondition();
            }));
        }
    }


    public void ResetGrid()
    {
        if (gridArray == null) return;

        // First pass: Clear all items
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridArray[x, y] != null)
                {
                    // Safely destroy the game object
                    if (gridArray[x, y].GameObject != null)
                    {
                        Destroy(gridArray[x, y].GameObject);
                    }
                    gridArray[x, y] = null;
                    isGridOccupied[x, y] = false;
                }
            }
        }

        // Second pass: Reset grid data
        gridArray = new ICellItem[0, 0];
        isGridOccupied = new bool[0, 0];
        width = 0;
        height = 0;

    }
    // Utility methods
    public bool IsGridAvailable(Vector3 position) { Vector2Int gridIndex = GetGridIndex(position); return IsInTheGrid(position) && !isGridOccupied[gridIndex.x, gridIndex.y]; }
    public bool IsInTheGrid(Vector3 position) { Vector2Int gridIndex = GetGridIndex(position); return IsValidPosition(gridIndex.x, gridIndex.y); }
    public ICellItem GetItemAt(int x, int y) => IsValidPosition(x, y) ? gridArray[x, y] : null;
    public bool IsValidPosition(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
    public Vector2Int GetGridIndex(Vector3 worldPosition) => new Vector2Int(Mathf.FloorToInt((worldPosition - transform.position).x / cellSize), Mathf.FloorToInt((worldPosition - transform.position).y / cellSize));
    public Vector3 GetGridPosition(int x, int y) => transform.position + new Vector3(x * cellSize, y * cellSize, 0);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i <= width; i++) { Vector3 start = transform.position + new Vector3(i * cellSize, 0, 0); Vector3 end = transform.position + new Vector3(i * cellSize, height * cellSize, 0); Gizmos.DrawLine(start, end); }
        for (int j = 0; j <= height; j++) { Vector3 start = transform.position + new Vector3(0, j * cellSize, 0); Vector3 end = transform.position + new Vector3(width * cellSize, j * cellSize, 0); Gizmos.DrawLine(start, end); }
    }
}