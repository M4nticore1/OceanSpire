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
            //Debug.Log($"{name} StartInteracting");
            StartInteracting();
            return;
        }
        if (ShouldBoatMoveToDock()) {
            //Debug.Log($"{name} BoatMoveToDock");
            BoatMoveToDock();
            return;
        }
        if (ShouldBoatFloatAway()) {
            //Debug.Log($"{name} BoatFloatAway");
            BoatFloatAway();
            return;
        }
        if (ShouldSetCombatTarget()) {
            //Debug.Log($"{name} StartAttacking");
            SetCombatTarget();
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

    //protected override void SetCombatTarget()
    //{
    //    AttackComponent.SetTarget(GetCurrentBuildingCombatTarget());
    //    AttackComponent.AddAttackers(GetCurrentBuildingCombatAttackers());
    //}

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

    //public override bool ShouldSetCombatTarget()
    //{
    //    if (!base.ShouldSetCombatTarget()) return false;

    //    return GetCurrentBuildingCombatTarget() != null;
    //}

    protected override void HandleInteractBuildingSeted(Building building)
    {
        building.RaidersHandler.AddInteractor(this);

        base.HandleInteractBuildingSeted(building);
    }

    protected override void HandleInteractBuildingRemoved(Building building)
    {
        building.RaidersHandler.RemoveInteractor(this);

        base.HandleInteractBuildingRemoved(building);
    }

    protected override void HandleInteractionStarted(Building building)
    {
        building.RaidersHandler.AddCurrentInteractor(this);
        StartRaidingBuilding();

        base.HandleInteractionStarted(building);
    }

    protected override void HandleInteractionStopped(Building building)
    {
        building.RaidersHandler.RemoveCurrentInteractor(this);
        StopRaidingBuilding();

        base.HandleInteractionStopped(building);
    }

    protected override void HandleEnteredBoat(Boat boat)
    {
        boat.SelectComponent.SetClickable(false);

        base.HandleEnteredBoat(boat);
    }

    protected override void HandleExitedBoat(Boat boat)
    {
        var interactBuilding = RaidManager.Instance.CalculateNextRaidBuilding();
        if (interactBuilding) {
            InteractComponent.SetInteractBuilding(interactBuilding);
        }

        base.HandleExitedBoat(boat);
    }

    protected override void HandleBoatSetedIdle(Boat boat)
    {
        base.HandleBoatSetedIdle(boat);

        BoatRider.StartExitingBoat();
    }

    protected override void HandleEnteredBuilding(Building buildng)
    {
        base.HandleEnteredBuilding(buildng);

        if (!IsRaidFinished) return;
        if (buildng != CityNavigator.TargetBuilding) return;

        CityNavigator.RemoveTargetBuilding();
        CityNavigator.RemoveTargetBuilding();
        CityNavigator.RemovePathAndTargetBuilding();
    }

    protected override void HandleAttackTargetSeted(AttackComponent combatComponent)
    {
        base.HandleAttackStarted(combatComponent);

        StopRaidingBuilding();
    }

    public override bool ShouldClick()
    {
        return false;
    }

    public float GetProgress()
    {
        if (raidBuildingTime == 0) return 0f;

        return currentRaidBuildingTime / raidBuildingTime;
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

        var interactBuilding = InteractComponent.InteractBuilding;
        if (interactBuilding == null) {
            Debug.LogError($"[{nameof(Raider)}] Interact Building is not valid!");
            return;
        }

        RaidManager.Instance.AddLosses(interactBuilding.GetRaidResources());
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

    //private AttackComponent GetCurrentBuildingCombatTarget()
    //{
    //    var currentBuilding = CityNavigator.CurrentBuilding;
    //    if (currentBuilding == null) return null;

    //    foreach (var worker in currentBuilding.CitizensHandler.CurrentInteractors) {
    //        if (worker == null) continue;
    //        if (worker.AttackComponent.CurrentTarget != null) continue;

    //        var citizen = worker.GetComponent<Citizen>();
    //        if (citizen == null) continue;
    //        if (!citizen.IsCitizenAvaliable()) continue;

    //        return worker.AttackComponent;
    //    }

    //    return null;
    //}

    //private List<AttackComponent> GetCurrentBuildingCombatAttackers()
    //{
    //    var currentBuilding = CityNavigator.CurrentBuilding;
    //    if (currentBuilding == null) return null;

    //    var attackers = new List<AttackComponent>();

    //    foreach (var worker in currentBuilding.CitizensHandler.CurrentInteractors) {
    //        if (worker == null) continue;

    //        var citizen = worker.GetComponent<Citizen>();
    //        if (citizen == null) continue;
    //        if (!citizen.IsCitizenAvaliable()) continue;

    //        attackers.Add(worker.AttackComponent);
    //    }

    //    return attackers;
    //}
}