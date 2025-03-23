using UnityEngine;

public class Rocket : MonoBehaviour, ICellItem
{
    private Vector2Int gridIndex;

    private int health = 1;
    public GameObject GameObject => gameObject;
    public string ItemType => "Rocket";

    public Vector2Int GridIndex
    {
        get => gridIndex;
        set => gridIndex = value;
    }
    public int Health
    {
        get => health;
        set => health = value;
    }

    public void OnTapped()
    {
        Debug.Log($"Rocket at {gridIndex} tapped!");
        // Later: Trigger match detection here
    }

    public bool CanFall()
    {
        return true;
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
            gameObject.SetActive(false);
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
