using UnityEngine;

public class WeaponCounter : EquipmentCounter<WeaponDefinition>
{
    public override int GetUsedCount(WeaponDefinition definition)
    {
        int amount = CityStorage.Instance.Inventory.GetItemById(definition.ItemId).Amount;

        foreach (Human human in CreaturesList.Instance.Citizens) {
            if (human.WeaponComponent.EquipmentDefinition != definition) continue;

            amount--;
        }

        return amount;
    }
}
