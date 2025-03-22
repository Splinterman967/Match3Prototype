using UnityEngine;

public interface ICellItem
{
    // The GameObject representing this item in the scene
    GameObject GameObject { get; }

    // The type of item (e.g., Cube, Rocket, Obstacle) for identification
    string ItemType { get; }

    // Position in the grid (x, y coordinates)
    Vector2Int GridPosition { get; set; }

    // Called when the item is tapped
    void OnTapped();

    // Called to check if the item can fall (e.g., cubes and vases can, stones cannot)
    bool CanFall();

    // Called to apply damage (e.g., obstacles take damage, rockets explode)
    void TakeDamage(int damage);

    // Called to destroy the item (e.g., when blasted or cleared)
    void DestroyItem();
}