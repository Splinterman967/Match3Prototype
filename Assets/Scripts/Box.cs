using UnityEngine;

public class Box : MonoBehaviour, ICellItem
{
    private Vector2Int gridIndex;

    public GameObject GameObject => gameObject;
    public string ItemType => "Box";
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
        Debug.Log($"Box at {gridIndex} tapped!");
        // Boxes don’t blast, so no action here (damage comes from adjacent blasts)
    }

    public bool CanFall()
    {
        return false; // Boxes don’t fall
    }

    public void TakeDamage(int damage)
    {
        DestroyItem(); // Box is cleared with 1 damage
    }

    public void DestroyItem()
    {
        gameObject.SetActive(false);
       // Destroy(gameObject);
    }

    public ICellItem[] GetNeighbours()
    {
        //Get the neighbours 
        ICellItem[] neighbourItems = { GridManager.Instance.GetItemAt(gridIndex.x + 1, gridIndex.y), GridManager.Instance.GetItemAt(gridIndex.x - 1, gridIndex.y)
                    , GridManager.Instance.GetItemAt(gridIndex.x, gridIndex.y + 1) , GridManager.Instance.GetItemAt(gridIndex.x, gridIndex.y - 1) };

        return neighbourItems;
    }
}