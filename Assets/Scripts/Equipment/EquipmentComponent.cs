using System;
using UnityEngine;

public class EquipmentComponent : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private EquipmentDefinition defaultEquipment;
    public EquipmentDefinition DefaultEquipment => defaultEquipment;

    public EquipmentDefinition EquipmentDefinition { get; private set; }
    public Equipment spawnedEquipment { get; private set; }

    private float powerBonus;

    public event Action<Equipment> OnEquipmentEquiped;
    public static event Action<EquipmentComponent> OnEquipmentComponentEquiped;

    public void Init()
    {
        var equipmenData = new EquipmentData()
        {
            EquipmentId = defaultEquipment?.ItemId
        };

        Init(equipmenData);
    }

    public void Init(EquipmentData equipmentData)
    {
        if (equipmentData == null) {
            Debug.LogError("Equipment Data is not valid");
            Init();
            return;
        }

        if (equipmentData.EquipmentId == null) return;

        int id = equipmentData.EquipmentId.Value;
        var definition = ItemsList.Instance.GetItem(id) as EquipmentDefinition;
        SetEquipmentAndApply(definition);
    }

    public void SetEquipmentAndApply(EquipmentDefinition definition)
    {
        SetWeaponDefinition(definition);
        TryDestroyEquipment();
        TrySpawnEquipment(definition);

        OnEquipmentComponentEquiped?.Invoke(this);
    }

    public void SetCurrentEquipmentVisible(bool value)
    {
        if (!spawnedEquipment) return;

        spawnedEquipment.gameObject.SetActive(value);
    }

    public void AddPowerBonus(float value)
    {
        powerBonus += value;
    }

    public void RemovePowerBonus(float value)
    {
        powerBonus -= value;
    }

    public float GetPower()
    {
        var power = EquipmentDefinition.Power;
        var bonusMultiplier = 1 + powerBonus;
        var powerWithBonus = power * bonusMultiplier;

        return powerWithBonus;
    }

    public bool EquipedDefaultEquipement()
    {
        return EquipmentDefinition == defaultEquipment;
    }

    private void SetWeaponDefinition(EquipmentDefinition definition)
    {
        EquipmentDefinition = definition;
        if (definition) return;

        EquipmentDefinition = defaultEquipment;
    }

    private void TrySpawnEquipment(EquipmentDefinition definition)
    {
        if (!definition) return;

        var prefab = definition.EquipmentPrefab;
        if (!prefab) return;

        spawnedEquipment = EquipmentFactory.CreateEquipment(prefab, spawnPoint);
    }

    private void TryDestroyEquipment()
    {
        if (!spawnedEquipment) return;

        Destroy(spawnedEquipment.gameObject);
    }
}