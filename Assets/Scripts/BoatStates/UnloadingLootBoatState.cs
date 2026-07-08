using Unity.Mathematics;
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
        if (boat.Inventory.Items.Count == 0) {
            currentWeightToUnload = 0f;
            return;
        }

        var loot = boat.TryGetItemToUnload();
        if (loot == null) return;

        var data = loot.Definition;
        var lootId = data.ItemId;
        float lootWeight = data.Weight;

        if (lootWeight <= 0f) {
            int allAmount = loot.Amount;
            boat.Inventory.RemoveItem(lootId, allAmount);
            currentWeightToUnload = 0f;
            return;
        }

        float weightToUnload = UnloadSpeed * Time.deltaTime;
        currentWeightToUnload += weightToUnload;

        int amountToUnload = math.min((int)(currentWeightToUnload / lootWeight), loot.Amount);
        if (amountToUnload == 0) return;

        boat.Inventory.RemoveItem(lootId, amountToUnload);
        currentWeightToUnload -= amountToUnload * lootWeight;
    }

    private bool ShouldUnload()
    {
        if (boat.Inventory.CurrentWeight <= 0f) return false;
        if (boat.Inventory.Items.Count == 0) return false;

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