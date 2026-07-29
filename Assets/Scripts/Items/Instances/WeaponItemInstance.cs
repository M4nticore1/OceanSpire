using System.Collections.Generic;
using UnityEngine;

public class WeaponItemInstance : ItemInstance
{
    public WeaponItemInstance(ItemDefinition definition) : base(definition)
    {
    }

    public override void RemoveAmount(int amount)
    {
        int amountBefore = Amount;
        base.RemoveAmount(amount);

        var cityStorage = CityStorage.Instance;
        if (!cityStorage) return;

        var cityStorageEquipment = cityStorage.Inventory.GetItem(Definition.ItemId);
        if (cityStorageEquipment != this) return;

        UnequipFromCitizens(amount);
    }

    private void UnequipFromCitizens(int totalToDiscard)
    {
        var creaturesManager = CreaturesManager.Instance;
        if (!creaturesManager) return;

        var citizens = creaturesManager.Citizens;
        if (citizens == null) return;

        var equippedCitizens = new List<Citizen>();
        for (int i = 0; i < citizens.Count; i++) {
            var citizen = citizens[i];
            if (!citizen) continue;
            if (!citizen.WeaponComponent) continue;

            if (citizen.WeaponComponent.EquipmentDefinition == Definition) {
                equippedCitizens.Add(citizen);
            }
        }

        int equippedCount = equippedCitizens.Count;
        int previousTotalAmount = Amount + totalToDiscard;
        int freeInStorage = Mathf.Max(0, previousTotalAmount - equippedCount);

        int amountToUnequip = totalToDiscard - freeInStorage;
        if (amountToUnequip <= 0) return;

        int unequippedCount = 0;
        for (int i = 0; i < equippedCitizens.Count; i++) {
            if (unequippedCount >= amountToUnequip) break;

            equippedCitizens[i].WeaponComponent.SetEquipmentAndApply(null);
            unequippedCount++;
        }
    }
}