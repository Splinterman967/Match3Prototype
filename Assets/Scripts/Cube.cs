using UnityEngine;
using static UnityEditor.Progress;

public class Cube : MonoBehaviour, ICellItem
{
    private Vector2Int gridIndex;
    private int health=1;
    
    [SerializeField] private ItemCode itemCode;
    public ItemCode ItemCode
    {
        get => itemCode;
        set => itemCode = value;
    }
    public ParticleSystem cubeParticle;
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
        Debug.Log($"Cube at {gridIndex} tapped! cube color : {itemCode} ");
        // Later: Trigger match detection here
    }
    public void OnBlast()
    {
        ICellItem[] neihbours = GetNeighbours();

        if (neihbours != null)
        {
            foreach (ICellItem item in neihbours)
            {
                if (item != null && item.ItemType == "Box")
                {
                    item.TakeDamage(1);
                }

                if(item != null && item.ItemType == "Stone")
                {
                    //Cannot be damaged with blast
                }

                if (item != null && item.ItemType == "Vase")
                {
                    item.TakeDamage(1);
                }
            }
        }
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
            if (cubeParticle != null)
            {
                Instantiate(cubeParticle, GridManager.Instance.GetGridPosition(gridIndex.x,gridIndex.y),Quaternion.identity);

            }
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