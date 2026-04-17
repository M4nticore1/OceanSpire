using System.Collections.Generic;
using UnityEngine;

public static class WeaponsDataGenerator
{
    public const int damagePerFloor = 2;

    public static WeaponHandlerData GetRandomDataGenerator(int maxDamage)
    {
        int weaponId = GetRandomWeaponId(maxDamage);
        WeaponHandlerData data = new WeaponHandlerData(weaponId);

        return data;
    }

    public static int GetMaxWeaponDamage()
    {
        int maxDamage = BuildingsManager.instance.BuiltFloors.Count * damagePerFloor;

        return maxDamage;
    }

    private static int GetRandomWeaponId(int maxDamage)
    {
        maxDamage = Mathf.Max(damagePerFloor, maxDamage);
        List<WeaponDefinition> weapons = new();

        foreach (var item in ItemsList.Instance.Items) {
            WeaponDefinition weapon = item as WeaponDefinition;
            if (!weapon) continue;

            if (weapon.Damage > maxDamage) continue;

            weapons.Add(weapon);
        }

        int index = Random.Range(0, weapons.Count);
        int id = weapons[index].ItemId;

        return id;
    }
}