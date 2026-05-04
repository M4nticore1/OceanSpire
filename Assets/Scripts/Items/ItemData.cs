using UnityEngine;

public class ItemData
{
    public int Id { get; private set; } = 0;
    public int Amount { get; private set; } = 0;

    public ItemData(int id, int amount)
    {
        Id = id;
        Amount = amount;
    }
}