using UnityEngine;

public class BonusChestData
{
    public long NextUpdateSeconds { get; private set; } = 0;
    public ItemData[] Items { get; private set; }

    public BonusChestData(long nextOpenSeconds, ItemData[] items)
    {
        NextUpdateSeconds = nextOpenSeconds;
        Items = items;
    }
}
