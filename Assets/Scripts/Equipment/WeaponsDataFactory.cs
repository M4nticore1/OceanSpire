using System.Collections.Generic;
using UnityEngine;

public static class WeaponsDataFactory
{
    public const int damagePerFloor = 2;

    public static EquipmentData CreateRandomData(float minDamage, float maxDamage)
    {
        int weaponId = GetRandomWeaponId(minDamage, maxDamage);
        var data = new EquipmentData()
        {
            EquipmentId = weaponId,
        };

        return data;
    }

    public static float GetMaxWeaponDamage()
    {
        int maxDamage = BuildingsManager.Instance.BuiltFloors.Count * damagePerFloor;
        return maxDamage;
    }

    public static float GetMinWeaponDamageId()
    {
        int id = 0;
        float maxDamage = 0;
        bool writed = false;

        foreach (var item in ItemsList.Instance.Items) {
            var weapon = item as WeaponDefinition;
            if (!weapon) continue;

            if (!writed) {
                maxDamage = weapon.Power;
                writed = true;
            }

            if (weapon.Power >= maxDamage) continue;

            maxDamage = weapon.Power;
            id = weapon.ItemId;
        }

        return maxDamage;
    }

    private static int GetRandomWeaponId(float minDamage, float maxDamage)
    {
        maxDamage = Mathf.Max(GetMinWeaponDamageId(), maxDamage);
        List<WeaponDefinition> weapons = new();

        foreach (var item in ItemsList.Instance.Items) {
            WeaponDefinition weapon = item as WeaponDefinition;
            if (!weapon) continue;

            if (weapon.Power < minDamage) continue;
            if (weapon.Power > maxDamage) continue;

            weapons.Add(weapon);
        }

        int index = Random.Range(0, weapons.Count);
        int id = weapons[index].ItemId;

        return id;
    }
}