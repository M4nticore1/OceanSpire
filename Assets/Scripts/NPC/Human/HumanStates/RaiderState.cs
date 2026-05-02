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
        CreaturesManager.Instance.RegisterRaider(human);

        human.Movement.SetMovementMethod(MovementMethod.Run);
        human.SelectComponent.SetClickable(false);
    }

    public override void Exit()
    {
        CreaturesManager.Instance.UnregisterRaider(human);
    }

    public override void Tick()
    {
        if (isRaidingBuilding) {
            ProcessRaidBuilding();
        }
    }

    private void ProcessRaidBuilding()
    {
        Building currentBuilding = human.CityNavigator.CurrentBuilding;
        if (!currentBuilding) return;

        Building interactBuilding = human.InteractComponent.InteractBuilding;
        if (currentBuilding != interactBuilding) return;

        currentRaidBuildingTime += Time.deltaTime;
        if (currentRaidBuildingTime < raidBuildingTime) return;

        FinishRaidingBuilding();
    }

    public override void OnAttackStarted()
    {

    }

    public override void OnAttackStopped()
    {
        if (!human.HealthComponent.IsAlive) return;

        UpdateRaidAction();
    }

    public override void OnSetedInteractBuilding(Building building)
    {
        if (building) {
            human.CityNavigator.TryFindPathToTargetBuilding();
            building.RaidComponent.AddRaider(human.InteractComponent);
        }
        else {
            human.MoveToBoat();
        }
    }

    public override void OnRemovedInteractBuilding(Building building)
    {
        human.InteractComponent.InteractBuilding.RaidComponent.RemoveRaider(human.InteractComponent);
    }

    public override void OnInteractionStarted()
    {
        human.InteractComponent.InteractBuilding.RaidComponent.EnterRaider(human.InteractComponent);
    }

    public override void OnInteractionStopped()
    {
        human.InteractComponent.InteractBuilding.RaidComponent.ExitRaider(human.InteractComponent);
    }

    public override void OnStoppedMoving()
    {

    }

    public override void OnEnteredBuilding(Building building)
    {
        if (isFinishedRaiding) return;
        if (building != human.InteractComponent.InteractBuilding) return;

        UpdateRaidAction();
    }

    public override void OnEnteredBoat(Boat boat)
    {
        if (isFinishedRaiding) {
            Vector3 position = RaidManager.Instance.GetSpawnPosition(human.BoatRider.selectedBoat);
            boat.FloatAway(position);
        }
        else {
            human.BoatRider.selectedBoat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    public override void OnExitedBoat(Boat boat)
    {
        Building interactBuilding = RaidManager.Instance.CalculateNextRaidBuilding();

        if (interactBuilding) {
            human.InteractComponent.SetInteractBuilding(interactBuilding);
        }
        else {
            FinishRaidingBuilding();
        }
    }

    public override void OnRevived()
    {

    }

    public override void OnDied()
    {
    }

    private void UpdateRaidAction()
    {
        if (ShouldAttackWorker()) {
            AttackWorker();
        }
        else if (ShouldRaidBuilding()) {
            StartRaidingBuilding();
        }
    }

    private void StartRaidingBuilding()
    {
        isRaidingBuilding = true;
    }

    private void AttackWorker()
    {
        Building building = human.CityNavigator.CurrentBuilding;
        AttackComponent target = building.WorkComponent.EnteredWorkers[0].GetComponent<AttackComponent>();

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
        IRaidable[] raidables = human.InteractComponent.InteractBuilding.GetComponents<IRaidable>();
        if (raidables == null) return;

        foreach (IRaidable raidable in raidables) {
            ItemInstance instance = raidable.GetRaidLoot();
            RaidManager.Instance.AddLose(instance);
        }
    }

    private bool ShouldAttackWorker()
    {
        Building building = human.CityNavigator.CurrentBuilding;
        if (building != human.InteractComponent.InteractBuilding) return false;

        if (building.WorkComponent.EnteredWorkers.Count == 0) return false;

        Human firstWorker = building.WorkComponent.EnteredWorkers[0].GetComponent<Human>();
        if (!firstWorker.HealthComponent.IsAlive) return false;

        if (building.WorkComponent.EnteredWorkers.Count <= 0) return false;

        return true;
    }

    private bool ShouldRaidBuilding()
    {
        Building building = human.CityNavigator.CurrentBuilding;
        if (building != human.InteractComponent.InteractBuilding) return false;

        return true;
    }
}