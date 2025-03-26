using DG.Tweening;
using UnityEngine;

public class Vase : MonoBehaviour, ICellItem
{
    [Header("Settings")]
    [SerializeField] private ItemCode itemCode = ItemCode.v;
    [SerializeField] private GameObject destructionParticles;
    [SerializeField] private GameObject intactVisual;
    [SerializeField] private GameObject damagedVisual;

    private Vector2Int gridIndex;
    private bool isBeingDestroyed;
    private bool damageTakenThisBlast;
    private int health = 2; // Takes 2 damages to clear

    public GameObject GameObject => gameObject;
    public string ItemType => "Obstacle";
    public ItemCode ItemCode { get => itemCode; set => itemCode = value; }
    public Vector2Int GridIndex { get => gridIndex; set => gridIndex = value; }
    public int Health
    {
        get => health;
        set => health = value; 
    }
    public bool IsBeingDestroyed() => isBeingDestroyed;
    public GameObject DestructionParticles => destructionParticles;

    private void UpdateVisualState()
    {
        bool isDamaged = health < 2;
        if (intactVisual != null) intactVisual.SetActive(!isDamaged);
        if (damagedVisual != null) damagedVisual.SetActive(isDamaged);
    }

    public void OnTapped()
    {
        if (isBeingDestroyed) return;
        transform.DOScale(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    public bool CanFall() => true; 

    public void TakeDamage(int damage)
    {
        if (isBeingDestroyed || damageTakenThisBlast) return;

        damageTakenThisBlast = true;
        Health -= damage; 

        if (Health <= 0)
        {
            isBeingDestroyed = true;
            GridManager.Instance.ClearItemAt(gridIndex.x, gridIndex.y);
        }
        else
        {
            UpdateVisualState();
        }
    }

    public void ResetDamageTracking()
    {
        damageTakenThisBlast = false;
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