using DG.Tweening;
using UnityEngine;

public class Box : MonoBehaviour, ICellItem
{
    [Header("Settings")]
    [SerializeField] private ItemCode itemCode = ItemCode.bo;
    [SerializeField] private GameObject destructionParticles;

    private Vector2Int gridIndex;
    private bool isBeingDestroyed;

    public GameObject GameObject => gameObject;
    public string ItemType => "Obstacle";
    public ItemCode ItemCode { get => itemCode; set => itemCode = value; }
    public Vector2Int GridIndex { get => gridIndex; set => gridIndex = value; }
    public int Health { get => 1; set { } } 
    public bool IsBeingDestroyed() => isBeingDestroyed;
    public GameObject DestructionParticles => destructionParticles;

    public void OnTapped()
    {
        if (isBeingDestroyed) return;
        transform.DOScale(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    public bool CanFall() => true; 

    public void TakeDamage(int damage)
    {
        if (isBeingDestroyed) return;
        isBeingDestroyed = true;
        GridManager.Instance.ClearItemAt(gridIndex.x, gridIndex.y);
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