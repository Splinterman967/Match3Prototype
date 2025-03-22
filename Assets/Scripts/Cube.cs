using UnityEngine;

public class Cube : MonoBehaviour, ICellItem
{
    public CubeType cubeType;

    private Vector2Int gridPosition;

    public GameObject GameObject => gameObject;
    public string ItemType => "Cube";

    public Vector2Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }

    public void OnTapped()
    {
        Debug.Log($"Cube at {gridPosition} tapped!");
        // Later: Trigger match detection here
    }

    public bool CanFall()
    {
        return true; // Cubes can fall
    }

    public void TakeDamage(int damage)
    {
        // Cubes don’t take damage traditionally; they blast instead
        DestroyItem();
    }

    public void DestroyItem()
    {
        gameObject.SetActive(false);
       // Destroy(gameObject);
    }
}