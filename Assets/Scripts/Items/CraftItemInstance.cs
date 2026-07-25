using System;
using UnityEngine;

public class CraftItemInstance
{
    public CraftItemDefinition Definition { get; private set; }

    public int CraftingTime { get; private set; } = 0;

    /// <summary>
    /// Эталонное время окончания БЕЗ бонусов (UtcNow + ProduceTime)
    /// </summary>
    public long? FinishTime { get; private set; } = null;

    public float SpeedBonus { get; private set; } = 0f;

    public event Action<float> OnSpeedBonusChanged;

    public CraftItemInstance(CraftItemDefinition definition, CraftItemData data)
    {
        Definition = definition;
        SetFinishTime(data.CraftingFinishTime);
    }

    /// <summary>
    /// Сколько ВСЕГО секунд должен длиться крафт с текущим бонусом
    /// </summary>
    public int GetCraftTimeWithBonus()
    {
        float safeBonus = Mathf.Clamp01(SpeedBonus);
        return Mathf.Max(0, (int)(Definition.ProduceTime * (1f - safeBonus)));
    }

    /// <summary>
    /// Честный остаток времени ДО ЗАВЕРШЕНИЯ (в реальных секундах)
    /// </summary>
    public int GetRemainingCraftingTime()
    {
        if (FinishTime == null) return 0;

        return GetCraftTimeWithBonus() - CraftingTime;
    }

    public bool IsCraftingFinished()
    {
        return GetRemainingCraftingTime() <= 0;
    }

    public void UpdateCraftingTime()
    {
        if (FinishTime == null) {
            SetCraftingTime(0);
            return;
        }

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long startTime = FinishTime.Value - Definition.ProduceTime;
        long passedBaseSeconds = currentTime - startTime;

        if (passedBaseSeconds <= 0) {
            SetCraftingTime(0);
            return;
        }

        int calculatedCraftingTime = (int)Mathf.Clamp(passedBaseSeconds, 0, Definition.ProduceTime);
        SetCraftingTime(calculatedCraftingTime);
    }

    public void SetCraftingTime(int time)
    {
        CraftingTime = Mathf.Clamp(time, 0, Definition.ProduceTime);
    }

    public void ResetFinishTime()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Сколько базовых секунд ещё осталось скрафтить
        int remainingBaseTime = Definition.ProduceTime - CraftingTime;

        SetFinishTime(currentTime + remainingBaseTime);
    }

    public void SetFinishTime(long? seconds)
    {
        FinishTime = seconds;
    }

    public void SetSpeedBonus(float value)
    {
        SpeedBonus = value;
        OnSpeedBonusChanged?.Invoke(value);
    }

    public long? GetFinishTimeWithBonus()
    {
        if (FinishTime == null) return null;

        float safeBonus = Mathf.Clamp01(SpeedBonus);
        int discountSeconds = (int)(Definition.ProduceTime * safeBonus);

        return FinishTime.Value - discountSeconds;
    }
}