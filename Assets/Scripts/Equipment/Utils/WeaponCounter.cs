using UnityEngine;

public class WeaponCounter : EquipmentCounter<WeaponDefinition>
{
    public override int GetUsedCount(WeaponDefinition definition)
    {
        int amount = CityStorage.Instance.Inventory.GetItemById(definition.ItemId).Amount;

        foreach (Citizen citizen in CreaturesList.Instance.Creatures) {
            if (!citizen) continue;
            if (citizen.WeaponComponent.EquipmentDefinition != definition) continue;

            amount--;
        }

        return amount;
    }
}