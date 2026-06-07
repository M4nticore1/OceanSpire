using UnityEngine;

public class Raider : Human
{
    [Header("Raider")]
    [SerializeField] private float raidBuildingTime = 10f;
    private float currentRaidBuildingTime = 0f;

    public bool IsRaidFinished { get; private set; } = false;
    private bool isRaidingBuilding = false;

    public Vector3 SpawnPosition { get; private set; } = Vector3.zero;

    protected override void OnEnable()
    {
        base.OnEnable();

        CreaturesManager.Instance.RegisterRaider(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CreaturesManager.Instance.UnregisterRaider(this);
    }

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

    protected override void OnInteractBuildingSeted(Building building)
    {
        if (building) {
            building.RaidComponent.AddRaider(this);
        }
        else {
            BoatRider.TryMoveToBoat();
        }

        UpdateRaidAction();

        base.OnInteractBuildingSeted(building);
    }

    protected override void OnInteractBuildingRemoved(Building building)
    {
        base.OnInteractBuildingRemoved(building);

        building.RaidComponent.RemoveRaider(this);
    }

    protected override void OnInteractionStarted(Building building)
    {
        base.OnInteractionStarted(building);

        building.RaidComponent.AddCurrentRaider(this);
    }

    protected override void OnInteractionStopped(Building building)
    {
        base.OnInteractionStopped(building);

        building.RaidComponent.RemoveCurrentRaider(this);
    }

    protected override void HandleEnteredBoat(Boat boat)
    {
        base.HandleEnteredBoat(boat);

        if (IsRaidFinished) {
            boat.FloatAway(SpawnPosition);
        }
        else {
            BoatRider.TargetBoat.SetState(BoatStateEnum.MovingToDock);
        }

        boat.SelectComponent.SetClickable(false);
    }

    protected override void HandleExitedBoat(Boat boat)
    {
        base.HandleExitedBoat(boat);

        var interactBuilding = RaidManager.Instance.CalculateNextRaidBuilding();

        if (interactBuilding) {
            InteractComponent.SetInteractBuilding(interactBuilding);
        }
    }

    protected override void OnBoatSetedIdle(Boat boat)
    {
        base.OnBoatSetedIdle(boat);

        BoatRider.StartExitingBoat();
    }

    protected override void OnAttackStopped()
    {
        base.OnAttackStopped();

        if (!HealthComponent.IsAlive) return;

        UpdateRaidAction();
    }

    public override bool ShouldClick()
    {
        return false;
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