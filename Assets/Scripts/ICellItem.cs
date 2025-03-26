using UnityEngine;

public interface ICellItem
{
    GameObject GameObject { get; }

    string ItemType { get; }

    ItemCode ItemCode { get; set; }

    int Health { get; set; }

    Vector2Int GridIndex { get; set; }

    void OnTapped();

    bool CanFall();

    void TakeDamage(int damage);

    ICellItem[] GetNeighbours();
    bool IsBeingDestroyed();

    GameObject DestructionParticles { get; }
}