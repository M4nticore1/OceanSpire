using UnityEngine;

public class DailyRewardData
{
    public long NextUpdateSeconds { get; private set; } = 0;
    public ItemData[] Items { get; private set; }

    public DailyRewardData(long nextOpenSeconds, ItemData[] items)
    {
        NextUpdateSeconds = nextOpenSeconds;
        Items = items;
    }
}
