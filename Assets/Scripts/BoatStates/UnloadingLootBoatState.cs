using System;
using UnityEngine;

public class UnloadingLootBoatState : BoatState, IProgressable
{
    public const float UnloadSpeed = 20f;
    private float stackedWeightToUnload = 0f;

    private CityStorage cityStorage = CityStorage.Instance;

    public static event Action<Boat, ItemInstance> OnLootUnloaded;

    public UnloadingLootBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        boat.RemoveTargetLoot();
    }

    public override void Exit()
    {

    }

    public override void Tick()
    {
        if (ShouldUnload()) {
            ProcessUnloadResources();
        }
        else {
            if (ShouldFindLoot()) {
                boat.SetState(BoatStateEnum.FindingLoot);
            }
            else {
                boat.SetState(BoatStateEnum.Idle);
            }
        }
    }

    public override void OnReachedPath()
    {

    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {

    }

    public float GetProgress()
    {
        if (boat.Inventory.WeightLimit == 0) return 0f;

        return 1f - (boat.Inventory.GetCurrentWeight() / boat.Inventory.WeightLimit);
    }

    private void ProcessUnloadResources()
    {
        if (!cityStorage) return;

        var loot = GetItemToUnload();
        if (loot == null) return;

        var lootAmount = loot.Amount;
        var lootData = loot.Definition;
        var lootId = lootData.ItemId;
        var lootWeight = lootData.Weight;

        int amountToUnload;

        if (lootWeight <= 0f) {
            amountToUnload = lootAmount;
        }
        else {
            var weightToUnload = UnloadSpeed * Time.deltaTime;
            stackedWeightToUnload += weightToUnload;

            amountToUnload = Mathf.Min((int)(stackedWeightToUnload / lootWeight), lootAmount);
            if (amountToUnload <= 0) return;

            stackedWeightToUnload -= amountToUnload * lootWeight;
        }

        if (amountToUnload <= 0) return;

        var itemData = new ItemData()
        {
            Id = lootId,
            Amount = amountToUnload
        };

        var item = ItemInstance.Create(itemData);

        boat.Inventory.RemoveItemAmount(item);
        cityStorage.Inventory.AddItemAmount(item);
        OnLootUnloaded?.Invoke(boat, item);
    }

    private ItemInstance GetItemToUnload()
    {
        foreach (var loot in boat.Inventory.Items) {
            if (loot == null) continue;
            if (loot.Amount <= 0) continue;

            return loot;
        }

        return null;
    }

    private bool ShouldUnload()
    {
        if (boat.Inventory.GetCurrentWeight() <= 0f) return false;
        if (GetItemToUnload() == null) return false;

        return true;
    }

    private bool ShouldFindLoot()
    {
        var rider = boat.CurrentRider;
        if (!rider) return false;
        if (!rider.TargetBoat) return false;
        if (rider.TargetBoat != boat) return false;
        if (!rider.RidingBoat) return false;
        if (!boat.CurrentRider.HealthComponent.IsAlive) return false;

        return true;
    }
}