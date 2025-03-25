using UnityEngine;

public class Stone : MonoBehaviour, ICellItem
{
    private Vector2Int gridIndex;

    public GameObject GameObject => gameObject;
    public string ItemType => "Obstacle";

    [SerializeField] private ItemCode itemCode;
    public ItemCode ItemCode
    {
        get => itemCode;
        set => itemCode = value;
    }
    public Vector2Int GridIndex
    {
        get => gridIndex;
        set => gridIndex = value;
    }
    private int health = 1;
    public int Health
    {
        get => health;
        set => health = value;
    }

    public void OnTapped()
    {
        Debug.Log($"Stone at {gridIndex} tapped!");
        // Boxes don’t blast, so no action here (damage comes from adjacent blasts)
    }

    public bool CanFall()
    {
        return false; // Boxes don’t fall
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        DestroyItem();
    }

    public void DestroyItem()
    {
        if (health <= 0)
        {
            GridManager.Instance.ClearItemAt(gridIndex.x, gridIndex.y);
        }
    }

    public ICellItem[] GetNeighbours()
    {
        //Get the neighbours 
        ICellItem[] neighbourItems = { GridManager.Instance.GetItemAt(gridIndex.x + 1, gridIndex.y), GridManager.Instance.GetItemAt(gridIndex.x - 1, gridIndex.y)
                    , GridManager.Instance.GetItemAt(gridIndex.x, gridIndex.y + 1) , GridManager.Instance.GetItemAt(gridIndex.x, gridIndex.y - 1) };

        return neighbourItems;
    }

}
