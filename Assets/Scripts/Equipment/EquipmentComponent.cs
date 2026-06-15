using System;
using UnityEngine;

public class EquipmentComponent : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private EquipmentDefinition defaultEquipment;
    public EquipmentDefinition DefaultEquipment => defaultEquipment;

    public EquipmentDefinition EquipmentDefinition { get; private set; }
    public Equipment spawnedEquipment { get; private set; }

    public static event Action<EquipmentComponent> OnEquipmentComponentEquiped;

    public void Init(EquipmentData data)
    {
        int id = data.EquipmentId;
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

    public float GetPower()
    {
        return EquipmentDefinition.Power;
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