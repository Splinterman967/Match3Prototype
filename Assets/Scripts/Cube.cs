using DG.Tweening;
using UnityEngine;

public class Cube : MonoBehaviour, ICellItem
{
    [Header("Settings")]
    [SerializeField] private ItemCode itemCode;
    [SerializeField] private int health = 1;
    [SerializeField] private GameObject destructionParticles;

    [Header("Animation")]
    [SerializeField] private float tapScaleAmount = 1.2f;
    [SerializeField] private float tapScaleDuration = 0.1f;

    private Vector2Int gridIndex;
    private bool isBeingDestroyed;

    public GameObject GameObject => gameObject;
    public string ItemType => "Cube";
    public ItemCode ItemCode { get => itemCode; set => itemCode = value; }
    public Vector2Int GridIndex { get => gridIndex; set => gridIndex = value; }
    public int Health { get => health; set => health = value; }
    public bool IsBeingDestroyed() => isBeingDestroyed;
    public GameObject DestructionParticles => destructionParticles;

    public void OnTapped()
    {
        if (isBeingDestroyed) return;
        transform.DOScale(tapScaleAmount, tapScaleDuration).SetLoops(2, LoopType.Yoyo);
    }

    public void OnBlast()
    {
        if (isBeingDestroyed) return;

        // Damage adjacent obstacles
        ICellItem[] neighbors = GetNeighbours();
        foreach (ICellItem neighbor in neighbors)
        {
            if (neighbor != null &&
                (neighbor.ItemCode == ItemCode.bo || neighbor.ItemCode == ItemCode.v))
            {
                neighbor.TakeDamage(1);
            }
        }

        TakeDamage(1);
    }

    public bool CanFall() => !isBeingDestroyed;

    public void TakeDamage(int damage)
    {
        if (isBeingDestroyed) return;

        health -= damage;
        if (health <= 0)
        {
            isBeingDestroyed = true;
            GridManager.Instance.ClearItemAt(gridIndex.x, gridIndex.y);
        }
    }

    public ICellItem[] GetNeighbours()
    {
        return new ICellItem[4]
        {
            GridManager.Instance.GetItemAt(gridIndex.x + 1, gridIndex.y),
            GridManager.Instance.GetItemAt(gridIndex.x - 1, gridIndex.y),
            GridManager.Instance.GetItemAt(gridIndex.x, gridIndex.y + 1),
            GridManager.Instance.GetItemAt(gridIndex.x, gridIndex.y - 1)
        };
    }
}