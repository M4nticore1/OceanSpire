using System;
using UnityEngine;

[Serializable]
public class EquipmentData
{
    public int EquipmentId = 0;

    public static EquipmentData Create(EquipmentComponent equipment)
    {
        return new EquipmentData()
        {
            EquipmentId = equipment.EquipmentDefinition.ItemId
        };
    }
}
