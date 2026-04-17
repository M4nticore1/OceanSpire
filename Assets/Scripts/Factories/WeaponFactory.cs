using UnityEngine;

public static class WeaponFactory
{
    public static Weapon CreateWeapon(Weapon prefab, Transform transform)
    {
        Weapon weapon = GameObject.Instantiate(prefab, transform);

        return weapon;
    }
}