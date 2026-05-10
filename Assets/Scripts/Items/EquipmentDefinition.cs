using UnityEngine;

public abstract class EquipmentDefinition : ItemDefinition
{
    [Header("Equipment")]
    [SerializeField] EquipmentCategory equipmentCategory;
    public EquipmentCategory EquipmentCategory => equipmentCategory;

    [SerializeField] private Equipment equipmentPrefab;
    public Equipment EquipmentPrefab => equipmentPrefab;

    [SerializeField] private float power = 0;
    public float Power => power;

    [SerializeField] private bool defaultEquipment = false;
    public bool DefaultEquipment => defaultEquipment;

    public abstract void Equip(Human human);
}
