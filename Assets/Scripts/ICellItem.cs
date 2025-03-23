using UnityEngine;

public interface ICellItem
{
    GameObject GameObject { get; }

    string ItemType { get; }

    int Health { get; set; }

    Vector2Int GridIndex { get; set; }

    void OnTapped();

    bool CanFall();

    void TakeDamage(int damage);

    void DestroyItem();

    ICellItem[] GetNeighbours();
}