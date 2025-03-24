using DG.Tweening;
using System.Collections;
using UnityEngine;
using static UnityEditor.Progress;

public class Rocket : MonoBehaviour, ICellItem
{
    public float rocketEffectDuration = 0.4f;
    private Vector2Int gridIndex;

    private int health = 1;
    public GameObject GameObject => gameObject;
    public string ItemType => "Rocket";

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
    public int Health
    {
        get => health;
        set => health = value;
    }

    public void OnTapped()
    {
        Debug.Log($"Rocket at {gridIndex} tapped!");
    }

    public bool CanFall()
    {
        return true;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        DestroyItem();

        StartCoroutine(RocketEffect());
    }

    public void DestroyItem()
    {
        if (health <= 0)
        {
            //GridManager.Instance.ClearItemAt(gridIndex.x, gridIndex.y);
        }

    
    }

    IEnumerator RocketEffect()
    {
        //When Animating hide the rocket
        GridManager.Instance.SetItemAt(gridIndex.x, gridIndex.y,null);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

        GridManager.Instance.Fall();

        if (itemCode == ItemCode.hro)
        {
            GameObject rocketLeft = gameObject.transform.GetChild(0).gameObject;
            GameObject rocketRight = gameObject.transform.GetChild(1).gameObject;

            rocketLeft.SetActive(true);
            rocketRight.SetActive(true);

            Vector3 targetPosLeft = GridManager.Instance.GetGridPosition(0, gridIndex.y);
            Vector3 targetPosRight = GridManager.Instance.GetGridPosition(GridManager.Instance.width - 1, gridIndex.y);

            // Animate movement with DOTween
            rocketLeft.transform.DOMove(targetPosLeft, rocketEffectDuration).SetEase(Ease.Linear);
            rocketRight.transform.DOMove(targetPosRight, rocketEffectDuration).SetEase(Ease.Linear);

            // Wait for the duration of the animation
            yield return new WaitForSeconds(rocketEffectDuration);

            // Deactivate rockets after they reach their targets
            rocketLeft.SetActive(false);
            rocketRight.SetActive(false);


        }

        else if( itemCode == ItemCode.vro)
        {
            GameObject rocketTop = gameObject.transform.GetChild(0).gameObject;
            GameObject rocketBottom = gameObject.transform.GetChild(1).gameObject;

            rocketTop.SetActive(true);
            rocketBottom.SetActive(true);

            Vector3 targetTop = GridManager.Instance.GetGridPosition(gridIndex.x, GridManager.Instance.height-1);
            Vector3 targetBottom = GridManager.Instance.GetGridPosition(gridIndex.x, 0);

            // Animate movement with DOTween
            rocketTop.transform.DOMove(targetTop, rocketEffectDuration).SetEase(Ease.Linear);
            rocketBottom.transform.DOMove(targetBottom, rocketEffectDuration).SetEase(Ease.Linear);

            // Wait for the duration of the animation
            yield return new WaitForSeconds(rocketEffectDuration);

            // Deactivate rockets after they reach their targets
            rocketTop.SetActive(false);
            rocketBottom.SetActive(false);

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
