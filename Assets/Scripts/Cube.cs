using UnityEngine;
using static UnityEditor.Progress;

public class Cube : MonoBehaviour, ICellItem
{
    private Vector2Int gridIndex;
    private int health=1;

    public CubeColor cubeColor;
    public GameObject GameObject => gameObject;
    public string ItemType => "Cube";

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
        Debug.Log($"Cube at {gridIndex} tapped! cube color : {cubeColor} ");
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
           GridManager.Instance.ClearItemAt(gridIndex.x,gridIndex.y);
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