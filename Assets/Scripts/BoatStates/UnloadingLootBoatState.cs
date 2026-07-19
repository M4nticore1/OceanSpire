using UnityEngine;

public class UnloadingLootBoatState : BoatState, IProgressable
{
    public const float UnloadSpeed = 20f;
    private float currentWeightToUnload = 0f;

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
            currentWeightToUnload += weightToUnload;

            amountToUnload = Mathf.Min((int)(currentWeightToUnload / lootWeight), lootAmount);
            if (amountToUnload < 1) return;

            currentWeightToUnload -= amountToUnload * lootWeight;
        }

        boat.Inventory.RemoveItem(lootId, amountToUnload);
        CityStorage.Instance.Inventory.AddItem(lootId, amountToUnload);
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