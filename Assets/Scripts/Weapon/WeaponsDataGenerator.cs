using System.Collections.Generic;
using UnityEngine;

public static class WeaponsDataGenerator
{
    public const int damagePerFloor = 2;

    public static WeaponHandlerData GetRandomDataGenerator(int minDamage, int maxDamage)
    {
        int weaponId = GetRandomWeaponId(minDamage, maxDamage);
        WeaponHandlerData data = new WeaponHandlerData(weaponId);

        return data;
    }

    public static int GetMaxWeaponDamage()
    {
        int maxDamage = BuildingsManager.instance.BuiltFloors.Count * damagePerFloor;

        return maxDamage;
    }

    public static int GetMinWeaponDamageId()
    {
        int id = 0;
        int maxDamage = 0;
        bool writed = false;

        foreach (var item in ItemsList.Instance.Items) {
            WeaponDefinition weapon = item as WeaponDefinition;
            if (!weapon) continue;

            if (!writed) {
                maxDamage = weapon.Damage;
                writed = true;
            }

            if (weapon.Damage >= maxDamage) continue;

            maxDamage = weapon.Damage;
            id = weapon.ItemId;
        }

        return maxDamage;
    }

    private static int GetRandomWeaponId(int minDamage, int maxDamage)
    {
        maxDamage = Mathf.Max(GetMinWeaponDamageId(), maxDamage);
        List<WeaponDefinition> weapons = new();

        foreach (var item in ItemsList.Instance.Items) {
            WeaponDefinition weapon = item as WeaponDefinition;
            if (!weapon) continue;

            if (weapon.Damage < minDamage) continue;
            if (weapon.Damage > maxDamage) continue;

            weapons.Add(weapon);
        }

        int index = Random.Range(0, weapons.Count);
        int id = weapons[index].ItemId;

        return id;
    }
}