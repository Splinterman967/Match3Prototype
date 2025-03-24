using UnityEngine;

public class Vase : MonoBehaviour, ICellItem
{
    private Vector2Int gridIndex;

    public GameObject GameObject => gameObject;
    public string ItemType => "Vase";


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
    private int health = 2;
    public int Health
    {
        get => health;
        set => health = value;
    }

    public void OnTapped()
    {
        Debug.Log($"Vase at {gridIndex} tapped!");
        // Boxes don’t blast, so no action here (damage comes from adjacent blasts)
    }

    public bool CanFall()
    {
        return true; // Boxes don’t fall
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        DestroyItem();
    }

    public void DestroyItem()
    {
        if (health == 1)
        {
            //Get the damaged state
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (health <= 0)
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
