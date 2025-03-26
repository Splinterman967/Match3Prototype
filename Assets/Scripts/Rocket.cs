using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class Rocket : MonoBehaviour, ICellItem
{
    [Header("Rocket Parts")]
    [SerializeField] private GameObject leftRocketPart;
    [SerializeField] private GameObject rightRocketPart;
    [SerializeField] private GameObject topRocketPart;
    [SerializeField] private GameObject bottomRocketPart;
    [SerializeField] private float rocketSpeed = 5f;
    [SerializeField] private ItemCode itemCode;

    private Vector2Int gridIndex;
    private bool isBeingDestroyed;
    private GridManager gridManager;
    private SpriteRenderer spriteRenderer;

    public GameObject GameObject => gameObject;
    public string ItemType => "Rocket";
    public ItemCode ItemCode { get => itemCode; set => itemCode = value; }
    public Vector2Int GridIndex { get => gridIndex; set => gridIndex = value; }
    public int Health { get => 1; set { } }
    public bool IsBeingDestroyed() => isBeingDestroyed;
    public GameObject DestructionParticles => null;

    private void Awake()
    {
        gridManager = GridManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnTapped()
    {
        if (isBeingDestroyed) return;

        List<Rocket> adjacentRockets = FindAdjacentRockets();
        if (adjacentRockets.Count > 0)
        {
            CreateRocketCombo(adjacentRockets);
        }
        else
        {
            ActivateRocketEffect();
        }
    }

    private List<Rocket> FindAdjacentRockets()
    {
        List<Rocket> rockets = new List<Rocket>();
        ICellItem[] neighbors = GetNeighbours();

        foreach (ICellItem neighbor in neighbors)
        {
            if (neighbor != null && neighbor.ItemType == "Rocket")
            {
                rockets.Add(neighbor as Rocket);
            }
        }

        return rockets;
    }

    private void CreateRocketCombo(List<Rocket> adjacentRockets)
    {
        isBeingDestroyed = true;
        adjacentRockets.Add(this);

        // Create 3x3 rocket parts explosion at each rocket position
        foreach (Rocket rocket in adjacentRockets)
        {
            rocket.isBeingDestroyed = true;
            Create3x3RocketParts(rocket.gridIndex,adjacentRockets);
            gridManager.ClearItemAt(rocket.gridIndex.x, rocket.gridIndex.y);
        }
    }

    private void Create3x3RocketParts(Vector2Int center, List<Rocket> adjacentRockets)
    {
        foreach(Rocket rocket in adjacentRockets)
        {
            rocket.isBeingDestroyed = true;
            rocket.GameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int targetPos = new Vector2Int(center.x + x, center.y + y);
                if (gridManager.IsValidPosition(targetPos.x, targetPos.y))
                {
                    // Launch rocket parts from each position in 3x3 area
              
                        LaunchRocketPart(leftRocketPart, new Vector2Int(0, targetPos.y));
                        LaunchRocketPart(rightRocketPart, new Vector2Int(gridManager.width - 1, targetPos.y));
                
                        LaunchRocketPart(topRocketPart, new Vector2Int(targetPos.x, gridManager.height - 1));
                        LaunchRocketPart(bottomRocketPart, new Vector2Int(targetPos.x, 0));
                    

                    // Damage cell at this position
                    ICellItem item = gridManager.GetItemAt(targetPos.x, targetPos.y);
                    if (item != null)
                    {
                        item.TakeDamage(1);
                    }
                }
            }
        }

        foreach (Rocket rocket in adjacentRockets)
        {
            Invoke(nameof(rocket.ClearFromGrid), 0.1f);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isBeingDestroyed) return;
        isBeingDestroyed = true;
        ActivateRocketEffect();
    }

    private void ActivateRocketEffect()
    {
        spriteRenderer.enabled = false;

        if (itemCode == ItemCode.hro) // Horizontal rocket
        {
            LaunchRocketPart(leftRocketPart, new Vector2Int(0, gridIndex.y));
            LaunchRocketPart(rightRocketPart, new Vector2Int(gridManager.width - 1, gridIndex.y));
        }
        else if (itemCode == ItemCode.vro) // Vertical rocket
        {
            LaunchRocketPart(topRocketPart, new Vector2Int(gridIndex.x, gridManager.height - 1));
            LaunchRocketPart(bottomRocketPart, new Vector2Int(gridIndex.x, 0));
        }

        Invoke(nameof(ClearFromGrid), 0.1f);
    }

    private void LaunchRocketPart(GameObject rocketPartPrefab, Vector2Int targetGridPos)
    {
        if (rocketPartPrefab == null) return;

        GameObject part = Instantiate(rocketPartPrefab, transform.position, Quaternion.identity);
        Vector3 targetPos = gridManager.GetGridPosition(targetGridPos.x, targetGridPos.y);
        float duration = Vector3.Distance(transform.position, targetPos) / rocketSpeed;

        part.transform.DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                DamageCellsAlongPath(targetGridPos);
                DestroyRocketPart(part);
            });
    }

    private void DamageCellsAlongPath(Vector2Int targetGridPos)
    {
        if (itemCode == ItemCode.hro)
        {
            int startX = Mathf.Min(gridIndex.x, targetGridPos.x);
            int endX = Mathf.Max(gridIndex.x, targetGridPos.x);

            for (int x = startX; x <= endX; x++)
            {
                DamageCell(x, gridIndex.y);
            }
        }
        else if (itemCode == ItemCode.vro)
        {
            int startY = Mathf.Min(gridIndex.y, targetGridPos.y);
            int endY = Mathf.Max(gridIndex.y, targetGridPos.y);

            for (int y = startY; y <= endY; y++)
            {
                DamageCell(gridIndex.x, y);
            }
        }
    }

    private void DestroyRocketPart(GameObject part)
    {
        ParticleSystem[] particles = part.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        Destroy(part, 0.5f); 
    }

    private void DamageCell(int x, int y)
    {
        if (gridManager.IsValidPosition(x, y))
        {
            ICellItem item = gridManager.GetItemAt(x, y);
            if (item != null && !item.IsBeingDestroyed())
            {
                item.TakeDamage(1);
            }
        }
    }

    private void ClearFromGrid()
    {
        gridManager.ClearItemAt(gridIndex.x, gridIndex.y);
    }

    public bool CanFall() => true;

    public ICellItem[] GetNeighbours()
    {
        return new ICellItem[4]
        {
            gridManager.GetItemAt(gridIndex.x + 1, gridIndex.y),
            gridManager.GetItemAt(gridIndex.x - 1, gridIndex.y),
            gridManager.GetItemAt(gridIndex.x, gridIndex.y + 1),
            gridManager.GetItemAt(gridIndex.x, gridIndex.y - 1)
        };
    }

    public void DestroyItem()
    {
        if (!isBeingDestroyed)
        {
            TakeDamage(1);
        }
    }
}