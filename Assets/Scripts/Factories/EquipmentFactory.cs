using UnityEngine;

public static class EquipmentFactory
{
    public static Equipment CreateEquipment(Equipment prefab, Transform transform)
    {
        Equipment weapon = GameObject.Instantiate(prefab, transform);

        return weapon;
    }
}