using UnityEngine;

public class Box : MonoBehaviour, ICellItem
{
    private Vector2Int gridPosition;

    public GameObject GameObject => gameObject;
    public string ItemType => "Box";
    public Vector2Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }

    public void OnTapped()
    {
        Debug.Log($"Box at {gridPosition} tapped!");
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
        Destroy(gameObject);
    }
}