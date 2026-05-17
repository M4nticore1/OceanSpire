using UnityEngine;

public class Raider : Human
{
    [SerializeField] private float raidBuildingTime = 10f;
    private float currentRaidBuildingTime = 0f;

    public bool IsRaidFinished { get; private set; } = false;
    private bool isRaidingBuilding = false;

    public Vector3 SpawnPosition { get; private set; } = Vector3.zero;

    protected override void Update()
    {
        base.Update();

        if (!isRaidingBuilding) return;

        Building currentBuilding = CityNavigator.CurrentBuilding;
        if (!currentBuilding) return;

        Building interactBuilding = InteractComponent.InteractBuilding;
        if (currentBuilding != interactBuilding) return;

        currentRaidBuildingTime += Time.deltaTime;
        if (currentRaidBuildingTime < raidBuildingTime) return;

        FinishRaidingBuilding();
    }

    protected override void OnInit(CreatureData data)
    {
        var raiderData = data as RaiderData;

        IsRaidFinished = raiderData.RaidFinished;
        SpawnPosition = raiderData.SpawnPosition.Vector3();

        Movement.SetMovementMethod(MovementMethod.Run);
        SelectComponent.SetClickable(false);

        base.OnInit(data);
    }

    protected override void OnEnteredBuilding(Building building)
    {
        base.OnEnteredBuilding(building);

        if (IsRaidFinished) return;
        if (building != InteractComponent.InteractBuilding) return;

        UpdateRaidAction();
    }

    protected override void OnSetedInteractBuilding(Building building)
    {
        base.OnSetedInteractBuilding(building);

        if (building) {
            building.RaidComponent.AddRaider(InteractComponent);
        }
        else {
            BoatRider.TryMoveToBoat();
        }
    }

    protected override void OnRemovedInteractBuilding(Building building)
    {
        base.OnRemovedInteractBuilding(building);

        building.RaidComponent.RemoveRaider(InteractComponent);
    }

    protected override void OnInteractionStarted(Building building)
    {
        base.OnInteractionStarted(building);

        building.RaidComponent.EnterRaider(InteractComponent);
    }

    protected override void OnInteractionStopped(Building building)
    {
        base.OnInteractionStopped(building);

        building.RaidComponent.EnterRaider(InteractComponent);
    }

    protected override void OnEnteredBoat(Boat boat)
    {
        base.OnEnteredBoat(boat);

        if (IsRaidFinished) {
            boat.FloatAway(SpawnPosition);
        }
        else {
            BoatRider.SelectedBoat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    protected override void OnExitedBoat(Boat boat)
    {
        base.OnExitedBoat(boat);

        var interactBuilding = RaidManager.Instance.CalculateNextRaidBuilding();

        if (interactBuilding) {
            InteractComponent.SetInteractBuilding(interactBuilding);
        }
    }

    protected override void OnAttackStopped()
    {
        base.OnAttackStopped();

        if (!HealthComponent.IsAlive) return;

        UpdateRaidAction();
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
        var building = CityNavigator.CurrentBuilding;
        var target = building.WorkComponent.EnteredWorkers[0].GetComponent<AttackComponent>();

        AttackComponent.SetTarget(target);
        AttackComponent.MoveToTarget();
    }

    private void FinishRaidingBuilding()
    {
        AddLoot();

        InteractComponent.RemoveInteractBuilding();
        BoatRider.TryMoveToBoat();

        isRaidingBuilding = false;
        IsRaidFinished = true;
    }

    private void AddLoot()
    {
        if (!InteractComponent) return;

        IRaidable[] raidables = InteractComponent.InteractBuilding.GetComponents<IRaidable>();
        if (raidables == null) return;

        foreach (IRaidable raidable in raidables) {
            ItemInstance instance = raidable.GetRaidLoot();
            RaidManager.Instance.AddLose(instance);
        }
    }

    private bool ShouldAttackWorker()
    {
        Building building = CityNavigator.CurrentBuilding;
        if (building != InteractComponent.InteractBuilding) return false;

        if (building.WorkComponent.EnteredWorkers.Count == 0) return false;

        Human firstWorker = building.WorkComponent.EnteredWorkers[0].GetComponent<Human>();
        if (!firstWorker.HealthComponent.IsAlive) return false;

        if (building.WorkComponent.EnteredWorkers.Count <= 0) return false;

        return true;
    }

    private bool ShouldRaidBuilding()
    {
        Building building = CityNavigator.CurrentBuilding;
        if (building != InteractComponent.InteractBuilding) return false;

        return true;
    }
}