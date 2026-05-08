using UnityEngine;

public enum EquipmentCategory
{
    Weapon,
    Armor
}

public enum AttackMethod
{
    Hands,
    Light,
    Heavy
}

[CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Items/WeaponDefinition")]
public class WeaponDefinition : EquipmentDefinition
{
    [Header("Weapon")]
    [SerializeField] private Weapon weaponPrefab;
    public Weapon WeaponPrefab => weaponPrefab;

    [SerializeField] private int damage;
    public int Damage => damage;

    [SerializeField] private AttackMethod attackMethods;
    public AttackMethod AttackMethods => attackMethods;

    public override void Equip(Human human)
    {
        human.WeaponEquipment.SetWeaponAndApply(this);
    }
}