using System;
using UnityEngine;

[Serializable]
public class ReviveSystemData
{
    public int RemainingRevivesCount = 0;
    public long[] NextReviveChargeTimes = Array.Empty<long>();

    public static ReviveSystemData Default()
    {
        return new ReviveSystemData();
    }

    public static ReviveSystemData Create(ReviveManager reviveManager)
    {
        if (!reviveManager) {
            Debug.LogError("reviveManager is not valid");
            return null;
        }

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var maxRevivesCount = reviveManager.MaxRevivesCount;
        var remainingRevivesCount = reviveManager.RemainingRevivesCount;

        var count = Math.Max(0, maxRevivesCount - remainingRevivesCount);
        var nextReviveChargeTimes = new long[count];

        if (count > 0) {
            var chargeTime = reviveManager.ChargeReviveTimeInSeconds;
            var nextChargeTime = reviveManager.NextChargeReviveTimeInSeconds;

            if (nextChargeTime == null) {
                Debug.LogError("nextChargeTime is not valid to create NextReviveChargeTimes");
                nextChargeTime = currentTime;
            }

            for (int i = 0; i < count; i++) {
                nextReviveChargeTimes[i] = nextChargeTime.Value;
                nextChargeTime += chargeTime;
            }
        }

        return new ReviveSystemData()
        {
            RemainingRevivesCount = remainingRevivesCount,
            NextReviveChargeTimes = nextReviveChargeTimes
        };
    }
}