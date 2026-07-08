using System;
using UnityEngine;

[Serializable]
public class EquipmentData
{
    public ItemID? EquipmentId = null;

    public static EquipmentData Default()
    {
        return new EquipmentData();
    }

    public static EquipmentData Create(EquipmentComponent equipment)
    {
        if (!equipment) {
            Debug.LogError("EquipmentComponent is not valid");
            return Default();
        }

        return new EquipmentData()
        {
            EquipmentId = equipment.EquipmentDefinition?.ItemId,
        };
    }
}