using UnityEngine;

public static class EquipmentUtils
{
    public static int GetMaxDamage(WeaponDefinition[] weaponDefinitions)
    {
        if (weaponDefinitions == null) {
            Debug.LogError("Weapon definitions array is not valid");
            return -1;
        }

        if (weaponDefinitions.Length <= 0) {
            Debug.LogError("Weapon definitions array length is 0");
            return -1;
        }

        int? minDamage = null;

        foreach (var definition in weaponDefinitions) {
            var damage = (int)definition.Power;

            if (minDamage == null) {
                minDamage = damage;
                continue;
            }

            if (damage < minDamage) continue;

            minDamage = damage;
        }

        return minDamage.Value;
    }

    public static int GetMinDamage(WeaponDefinition[] weaponDefinitions)
    {
        if (weaponDefinitions == null) {
            Debug.LogError("Weapon definitions array is not valid");
            return -1;
        }

        if (weaponDefinitions.Length <= 0) {
            Debug.LogError("Weapon definitions array length is 0");
            return -1;
        }

        int? minDamage = null;

        foreach (var definition in weaponDefinitions) {
            var damage = (int)definition.Power;

            if (minDamage == null) {
                minDamage = damage;
                continue;
            }

            if (damage > minDamage) continue;

            minDamage = damage;
        }

        return minDamage.Value;
    }
}