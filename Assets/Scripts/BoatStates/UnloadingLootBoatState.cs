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
            ProcessStoreResources();
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

        return boat.CurrentWeight / boat.MaxWeight;
    }

    private void ProcessStoreResources()
    {
        if (boat.Inventory.Items.Count == 0) {
            Debug.LogError("items count is 0.");
            return;
        }

        // Item
        ItemInstance loot = boat.TryGetItemToUnload();
        ItemDefinition data = loot.Definition;
        int lootId = data.ItemId;
        float lootWeight = data.Weight;

        // Weight
        float weightToUnload = UnloadSpeed * Time.deltaTime;
        currentWeightToUnload += weightToUnload;
        int amountToUnload = math.min((int)(currentWeightToUnload / lootWeight), loot.Amount);

        if (amountToUnload == 0) return;

        // Spend Item
        boat.Inventory.RemoveItem(lootId, amountToUnload);
        currentWeightToUnload = 0f;

        EventBus.InvokeBoatUnloadedItem(lootId, amountToUnload);
    }

    private bool ShouldUnload()
    {
        return boat.Inventory.CurrentWeight > 0;
    }

    private bool ShouldFindLoot()
    {
        if (!boat.CurrentRider) return false;
        if (!boat.CurrentRider.HealthComponent.IsAlive) return false;

        return true;
    }
}