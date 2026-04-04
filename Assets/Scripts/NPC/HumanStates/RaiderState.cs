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

    private void StartRaidingBuilding()
    {
        isRaidingBuilding = true;
    }

    private void FinishRaidingBuilding()
    {
        human.MoveToBoat();
        isRaidingBuilding = false;
        isFinishedRaiding = true;
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
        else if (building != human.Interactor.interactBuilding) {
            if (building.currentWorkers.Count > 0) {
                Health target = building.currentWorkers[0].GetComponent<Health>();
                human.Attack.SetTarget(target);
                human.Attack.MoveToTarget();
            }
            else {
                StartRaidingBuilding();
            }
        }
    }

    public override void OnEnteredBoat(Boat boat)
    {
        if (!isFinishedRaiding) return;

        Vector3 position = RaidManager.instance.GetSpawnPosition(human.BoatRider.selectedBoat);
        boat.FloatAway(position);
    }
}