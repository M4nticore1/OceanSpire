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
    [SerializeField] private AttackMethod attackMethod;
    public AttackMethod AttackMethod => attackMethod;

    public override void Equip(Human human)
    {
        human.WeaponComponent.SetEquipmentAndApply(this);
    }

    public override ItemInstance CreateInstance()
    {
        return new WeaponItemInstance(this);
    }
}