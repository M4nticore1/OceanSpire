using UnityEngine;

public class RaiderState : HumanState
{
    private const float raidBuildingTime = 10f;
    private float currentRaidBuildingTime = 0f;
    private bool isRaidingBuilding = false;
    public bool isFinishedRaiding { get; private set; } = false;

    public RaiderState(Human human) : base(human)
    {

    }

    public override void Enter()
    {
        CreaturesManager.instance.RegisterRaider(human);

        human.Movement.SetMovementMethod(MovementMethod.Run);
    }

    public override void Exit()
    {
        CreaturesManager.instance.UnregisterRaider(human);
    }

    public override void Tick()
    {
        if (isRaidingBuilding) {
            ProcessRaidBuilding();
        }
    }

    private void ProcessRaidBuilding()
    {
        Building currentBuilding = human.CityNavigator.currentBuilding;
        if (!currentBuilding) return;

        Building interactBuilding = human.InteractComponent.interactBuilding;
        if (currentBuilding != interactBuilding) return;

        currentRaidBuildingTime += Time.deltaTime;
        if (currentRaidBuildingTime < raidBuildingTime) return;

        FinishRaidingBuilding();
    }

    public override void OnStartedAttacking()
    {

    }

    public override void OnStoppedAttacking()
    {
        if (!human.HealthComponent.isAlive) return;

        UpdateRaidAction();
    }

    public override void OnSetedInteractBuilding(Building building)
    {
        if (building) {
            human.CityNavigator.TryFindPathToTargetBuilding();
        }
        else {
            human.MoveToBoat();
        }
    }

    public override void OnRemovedInteractBuilding()
    {
        human.InteractComponent.RemoveInteractBuilding();
    }

    public override void OnStoppedMoving()
    {

    }

    public override void OnEnteredBuilding(Building building)
    {
        if (!isFinishedRaiding && building == human.InteractComponent.interactBuilding) {
            UpdateRaidAction();
        }
    }

    public override void OnEnteredBoat(Boat boat)
    {
        if (isFinishedRaiding) {
            Vector3 position = RaidManager.instance.GetSpawnPosition(human.BoatRider.selectedBoat);
            boat.FloatAway(position);
        }
        else {
            human.BoatRider.selectedBoat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    public override void OnExitedBoat(Boat boat)
    {
        Building interactBuilding = RaidManager.GetRandomRaidBuilding();
        human.SetInteractBuilding(interactBuilding);
    }

    public override void OnRevived()
    {

    }

    public override void OnDied()
    {
        EventBus.InvokeRaiderDied(human);
    }

    public override void OnDisable()
    {
        CreaturesManager.instance.UnregisterRaider(human);
    }

    private void UpdateRaidAction()
    {
        if (ShouldAttackWorker()) {
            StartAttackingWorker();
        }
        else if (ShouldRaidBuilding()) {
            StartRaidingBuilding();
        }
    }

    private void StartRaidingBuilding()
    {
        isRaidingBuilding = true;
    }

    private void StartAttackingWorker()
    {
        Building building = human.CityNavigator.currentBuilding;
        AttackComponent target = building.currentWorkers[0].GetComponent<AttackComponent>();

        human.AttackComponent.SetTarget(target);
        human.AttackComponent.MoveToTarget();
    }

    private void FinishRaidingBuilding()
    {
        human.MoveToBoat();
        AddLoot();
        isRaidingBuilding = false;
        isFinishedRaiding = true;
    }

    private void AddLoot()
    {
        ItemInstance instance = human.InteractComponent.interactBuilding.GetComponentInChildren<IRaidable>().GetRaidLoot();
        RaidManager.instance.AddLose(instance);
    }

    private bool ShouldAttackWorker()
    {
        Building building = human.CityNavigator.currentBuilding;
        if (building != human.InteractComponent.interactBuilding) return false;

        if (building.currentWorkers.Count == 0) return false;

        Human firstWorker = building.currentWorkers[0].GetComponent<Human>();
        if (!firstWorker.HealthComponent.isAlive) return false;

        return building.currentWorkers.Count > 0;
    }

    private bool ShouldRaidBuilding()
    {
        Building building = human.CityNavigator.currentBuilding;
        if (building != human.InteractComponent.interactBuilding) return false;

        return true;
    }
}