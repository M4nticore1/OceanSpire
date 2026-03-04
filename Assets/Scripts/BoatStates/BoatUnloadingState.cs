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

    public override void Process()
    {
        Debug.Log("BoatUnloadingState");
        boat.ProcessDrainHealth();

        if (ShouldUnload()) {
            Debug.Log("ShouldUnload");
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

    public override void HandleReachedPath()
    {

    }

    private void ProcessStoreResources()
    {
        if (boat.Inventory.items.Count == 0) {
            Debug.LogError("items count is 0.");
            return;
        }

        Debug.Log("ProcessStoreResources");

        // Item
        ItemInstance loot = boat.GetItemToUnload();
        ItemData data = loot.ItemData;
        int lootId = data.ItemId;
        float lootWeight = data.Weight;

        // Weight
        float weightToUnload = UnloadSpeed * Time.deltaTime;
        currentWeightToUnload += weightToUnload;
        int amountToUnload = math.min((int)(currentWeightToUnload / lootWeight), loot.Amount);

        if (amountToUnload == 0) return;

        // Spend Item
        boat.Inventory.RemoveItemAmount(lootId, amountToUnload);
        currentWeightToUnload = 0f;

        EventBus.InvokeBoatUnloadedItem(lootId, amountToUnload);
    }

    private bool ShouldUnload()
    {
        return boat.Inventory.CurrentWeight > 0;
    }

    private bool ShouldExit()
    {
        EntityInteractor interactor = boat.rider.GetComponent<EntityInteractor>();
        return !interactor.InteractBuilding || BoatsManager.Instance.GetBoatByInteractorIndex(interactor.interactorIndex) != boat;
    }
}