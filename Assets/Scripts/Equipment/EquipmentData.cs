using UnityEngine;

public class EquipmentData
{
    public int WeaponId { get; private set; } = 0;

    public EquipmentData(int weaponId)
    {
        WeaponId = weaponId;
    }
}
