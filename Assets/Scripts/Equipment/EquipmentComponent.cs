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
        if (!defaultEquipment) {
            Debug.LogError($"DefaultEquipment is not valid at {name}");
        }

        var equipmenData = new EquipmentData()
        {
            EquipmentId = defaultEquipment?.ItemId
        };

        Init(equipmenData);
    }

    public void Init(EquipmentData equipmentData)
    {
        if (equipmentData == null || equipmentData.EquipmentId == null) {
            Debug.LogError($"[{nameof(EquipmentComponent)}] Equipment Data or Equipment Id is not valid");
            Init();
            return;
        }

        if (equipmentData.EquipmentId == null) return;

        var id = equipmentData.EquipmentId.Value;
        var definition = ItemsList.Instance.GetItem(id) as EquipmentDefinition;
        SetEquipmentAndApply(definition ? definition : defaultEquipment);
    }

    public void SetEquipmentAndApply(EquipmentDefinition definition)
    {
        var targetDefinition = definition ? definition : defaultEquipment;

        SetWeaponDefinition(targetDefinition);
        TryDestroyEquipment();
        TrySpawnEquipment(targetDefinition);

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
        if (!EquipmentDefinition) {
            Debug.LogError($"[{nameof(EquipmentComponent)}] Equipment Defintion is not valid!");
            return defaultEquipment ? defaultEquipment.Power : 1;
        }

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
        if (definition) {
            EquipmentDefinition = definition;
        }
        else {
            EquipmentDefinition = defaultEquipment;
        }

        if (!EquipmentDefinition) {
            Debug.LogError($"[{nameof(EquipmentComponent)}] Equipment Defintion is not valid!");
        }
    }

    private void TrySpawnEquipment(EquipmentDefinition definition)
    {
        if (!definition) {
            Debug.LogError($"[{nameof(EquipmentComponent)}] Equipment Defintion is not valid!");
            return;
        }

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