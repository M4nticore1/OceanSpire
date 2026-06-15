using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BoatUnloadingState : BoatState
{
    public const float UnloadSpeed = 20f;
    private float currentWeightToUnload = 0f;

    public BoatUnloadingState(Boat boat) : base(boat)
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
            if (ShouldExit()) {
                boat.SetState(BoatStateEnum.Idle);
            }
            else {
                boat.SetState(BoatStateEnum.FindingLoot);
            }
        }
    }

    public override void OnReachedPath()
    {

    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {

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

    private bool ShouldExit()
    {
        if (!boat.CurrentRider) return false;

        return true;
    }
}