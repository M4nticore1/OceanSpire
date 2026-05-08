using UnityEngine;

public abstract class EquipmentDefinition : ItemDefinition
{
    [Header("Equipment")]
    [SerializeField] EquipmentCategory category;
    public EquipmentCategory Category => category;

    [SerializeField] private float power = 0;
    public float Power => power;

    [SerializeField] private bool defaultEquipment = false;
    public bool DefaultEquipment => defaultEquipment;

    public abstract void Equip(Human human);
}
