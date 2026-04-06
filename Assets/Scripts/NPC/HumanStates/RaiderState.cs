using UnityEngine;
using UnityEngine.UIElements;

public class RaiderState : HumanState
{
    private const float raidBuildingTime = 10f;
    private float currentRaidBuildingTime = 0f;
    private bool isRaidingBuilding = false;
    public bool isFinishedRaiding { get; private set; } = false;

    public RaiderState(Human human) : base(human)
    {

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

        Building interactBuilding = human.Interactor.interactBuilding;
        if (currentBuilding != interactBuilding) return;

        currentRaidBuildingTime += Time.deltaTime;
        if (currentRaidBuildingTime < raidBuildingTime) return;

        FinishRaidingBuilding();
    }

    public override void OnStoppedAttacking()
    {
        UpdateRaidAction();
    }

    public override void OnSetedInteractBuilding(Building building)
    {
        if (human.BoatRider.isRidingOnBoat) {
            human.BoatRider.selectedBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            human.CityNavigator.TryFindPathToTargetBuilding();
        }
    }

    public override void OnRemovedInteractBuilding()
    {
        human.Interactor.RemoveInteractBuilding();
    }

    public override void OnStoppedMoving()
    {

    }

    public override void OnEnteredBuilding(Building building)
    {
        if (isFinishedRaiding) {
            if (building == human.CityNavigator.targetBuilding) {
                human.MoveToBoat();
            }
        }
        else if (building = human.Interactor.interactBuilding) {
            UpdateRaidAction();
        }
    }

    public override void OnEnteredBoat(Boat boat)
    {
        if (!isFinishedRaiding) return;

        Vector3 position = RaidManager.instance.GetSpawnPosition(human.BoatRider.selectedBoat);
        boat.FloatAway(position);
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
        Attack target = building.currentWorkers[0].GetComponent<Attack>();

        human.Attack.SetTarget(target);
        human.Attack.MoveToTarget();
    }

    private void FinishRaidingBuilding()
    {
        human.MoveToBoat();
        isRaidingBuilding = false;
        isFinishedRaiding = true;
    }

    private bool ShouldAttackWorker()
    {
        Building building = human.CityNavigator.currentBuilding;
        if (building != human.Interactor.interactBuilding) return false;

        if (building.currentWorkers.Count == 0) return false;

        Human firstWorker = building.currentWorkers[0].GetComponent<Human>();
        if (!firstWorker.Health.isAlive) return false;

        Debug.Log("ShouldAttackWorker");
        return building.currentWorkers.Count > 0;
    }

    private bool ShouldRaidBuilding()
    {
        Building building = human.CityNavigator.currentBuilding;
        if (building != human.Interactor.interactBuilding) return false;

        Debug.Log("ShouldRaidBuilding");
        return true;
    }

    public override void OnDeath()
    {

    }
}