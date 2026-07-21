using System;
using UnityEngine;

public class UnloadingLootBoatState : BoatState, IProgressable
{
    public const float UnloadSpeed = 20f;
    private float stackedWeightToUnload = 0f;

    private CityStorage cityStorage = CityStorage.Instance;

    public static event Action<ItemID, int> OnLootUnloaded;

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
        if (boat.MaxWeight == 0) return 0f;

        return 1f - (boat.CurrentWeight / boat.MaxWeight);
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

        boat.Inventory.RemoveItem(lootId, amountToUnload);
        cityStorage.Inventory.AddItem(lootId, amountToUnload);
        OnLootUnloaded?.Invoke(lootId, amountToUnload);
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
        if (boat.Inventory.CurrentWeight <= 0f) return false;
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