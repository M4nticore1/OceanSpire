using System;
using UnityEngine;

public class Raider : Human, IProgressable
{
    [Header("Raider")]
    [SerializeField] private float raidBuildingTime = 10f;
    private float currentRaidBuildingTime = 0f;

    public bool IsRaidFinished { get; private set; } = false;
    public bool IsRaidingBuilding { get; private set; } = false;

    public Vector3 SpawnPosition { get; private set; } = Vector3.zero;

    public event Action<Building> OnRaidBuildingStarted;
    public event Action<Building> OnRaidBuildingStopped;

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

    public override void Tick()
    {
        base.Tick();

        if (IsRaidingBuilding) {
            currentRaidBuildingTime += Time.deltaTime;
            if (currentRaidBuildingTime < raidBuildingTime) return;

            AddLoot();
            StopRaidingBuilding();
            FinishRaid();

            var interactBuilding = InteractComponent.InteractBuilding;
            InteractComponent.TryStopInteracting(interactBuilding);
            InteractComponent.RemoveInteractBuilding();

            UpdateTargetBoat();

            var targetBoat = BoatRider.TargetBoat;
            if (!targetBoat) {
                Debug.LogError($"[{nameof(Raider)}] Target Boat is not valid!");
                Destroy(gameObject);
            }
        }
    }

    protected override void HandleInit(CreatureData data)
    {
        var raiderData = data as RaiderData;

        IsRaidFinished = raiderData.RaidFinished;
        SpawnPosition = raiderData.SpawnPosition.Vector3();

        Movement.SetMovementMethod(MovementMethod.Run);
        SelectComponent.SetClickable(false);

        base.HandleInit(data);
    }

    protected override CreatureData GetDefaultData()
    {
        return RaiderData.Default();
    }

    protected override void DetermineNextAction()
    {
        if (ShouldStartInteracting()) {
            Debug.Log($"{name} StartInteracting");
            StartInteracting();
            return;
        }
        if (ShouldBoatMoveToDock()) {
            Debug.Log($"{name} BoatMoveToDock");
            BoatMoveToDock();
            return;
        }
        if (ShouldBoatFloatAway()) {
            Debug.Log($"{name} BoatFloatAway");
            BoatFloatAway();
            return;
        }
        if (ShouldStartAttacking()) {
            Debug.Log($"{name} StartAttacking");
            StartAttacking();
            return;
        }

        base.DetermineNextAction();
    }

    protected override void BoatFloatAway()
    {
        base.BoatFloatAway();

        var boat = BoatRider.RidingBoat;
        boat.FloatAway(SpawnPosition);
        boat.RemoveDockPoint();
    }

    protected override void StartAttacking()
    {
        var currentBuilding = CityNavigator.CurrentBuilding;
        var currentWorkers = currentBuilding.CitizensHandler.CurrentInteractors;

        foreach (var worker in currentWorkers) {
            var citizen = worker as Citizen;
            if (!citizen) continue;
            if (!citizen.IsCitizenAvaliable()) continue;

            AttackComponent.SetTarget(worker.AttackComponent);
            AttackComponent.MoveToTarget();
            break;
        }
    }

    protected override void StartInteracting()
    {
        base.StartInteracting();

        CityNavigator.FollowPath();
    }

    public override bool ShouldStartInteracting()
    {
        if (!base.ShouldStartInteracting()) return false;
        if (IsRaidFinished) return false;
        if (IsRaidingBuilding) return false;

        return true;
    }

    public override bool ShouldBoatMoveToDock()
    {
        if (!base.ShouldBoatMoveToDock()) return false;
        if (IsRaidFinished) return false;

        return true;
    }

    public override bool ShouldBoatFloatAway()
    {
        if (!base.ShouldBoatFloatAway()) return false;
        if (!IsRaidFinished) return false;

        return true;
    }

    public override bool ShouldStartAttacking()
    {
        if (!base.ShouldStartAttacking()) return false;

        var currentBuilding = CityNavigator.CurrentBuilding;
        if (!currentBuilding) return false;

        if (currentBuilding != InteractComponent.InteractBuilding) return false;

        var currentWorkers = currentBuilding.CitizensHandler.CurrentInteractors;
        foreach (var worker in currentWorkers) {
            var citizen = worker as Citizen;
            if (!citizen) continue;
            if (!citizen.IsCitizenAvaliable()) continue;

            return true;
        }

        return false;
    }

    protected override void OnInteractBuildingSeted(Building building)
    {
        building.RaidersHandler.AddInteractor(this);

        base.OnInteractBuildingSeted(building);
    }

    protected override void OnInteractBuildingRemoved(Building building)
    {
        building.RaidersHandler.RemoveInteractor(this);

        base.OnInteractBuildingRemoved(building);
    }

    protected override void OnInteractionStarted(Building building)
    {
        building.RaidersHandler.AddCurrentInteractor(this);
        StartRaidingBuilding();

        base.OnInteractionStarted(building);
    }

    protected override void OnInteractionStopped(Building building)
    {
        building.RaidersHandler.RemoveCurrentInteractor(this);
        StopRaidingBuilding();

        base.OnInteractionStopped(building);
    }

    protected override void HandleEnteredBoat(Boat boat)
    {
        base.HandleEnteredBoat(boat);

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

    protected override void OnEnteredBuilding(Building buildng)
    {
        base.OnEnteredBuilding(buildng);

        if (!IsRaidFinished) return;
        if (buildng != CityNavigator.TargetBuilding) return;

        CityNavigator.RemoveTargetBuilding();
        CityNavigator.RemoveTargetBuilding();
        CityNavigator.RemovePath();
    }

    protected override void OnAttackStarted()
    {
        base.OnAttackStarted();

        StopRaidingBuilding();
    }

    public override bool ShouldClick()
    {
        return false;
    }

    private void StartRaidingBuilding()
    {
        IsRaidingBuilding = true;
        currentRaidBuildingTime = 0f;
        OnRaidBuildingStarted?.Invoke(CityNavigator.CurrentBuilding);
    }

    private void StopRaidingBuilding()
    {
        IsRaidingBuilding = false;
        OnRaidBuildingStopped?.Invoke(CityNavigator.CurrentBuilding);
    }

    private void FinishRaid()
    {
        IsRaidFinished = true;
    }

    private void AddLoot()
    {
        if (!InteractComponent) return;

        var raidables = InteractComponent.InteractBuilding.GetComponents<IRaidable>();
        if (raidables == null) return;

        foreach (IRaidable raidable in raidables) {
            var loot = raidable.GetRaidLoot();

            foreach (var items in loot) {
                RaidManager.Instance.AddLose(items);
            }
        }
    }

    private void UpdateTargetBoat()
    {
        var raiderBoats = BoatsManager.Instance.RaiderBoats;

        for (int i = 0; i < raiderBoats.Count; i++) {
            var boat = raiderBoats[i];
            if (!boat) {
                Debug.LogError($"[{nameof(Raider)}] Raider Boat is not valid at index {i}");
                continue;
            }

            if (boat.CurrentRider) continue;

            var targetRidet = boat.TargetRider;
            if (targetRidet && targetRidet != BoatRider) continue;

            BoatRider.TrySetTargetBoat(boat);
            return;
        }

        Debug.LogError($"[{nameof(Raider)}] No free raid boats available of {raiderBoats.Count} boats!");
    }

    public float GetProgress()
    {
        if (raidBuildingTime == 0) return 0f;

        return currentRaidBuildingTime / raidBuildingTime;
    }
}