using UnityEngine;

public abstract class EquipmentCounter<T>
{
    public abstract int GetUsedCount(T definition);
}
