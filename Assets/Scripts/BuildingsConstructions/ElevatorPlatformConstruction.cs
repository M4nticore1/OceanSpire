using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPlatformConstruction : BuildingConstruction
{
    private CityManager cityManager = null;

    //public List<Entity> walkingPassengers { get; private set; } = new List<Entity>();
    public List<Entity> waitingPassengers = new List<Entity>();
    public List<Entity> ridingPassengers = new List<Entity>();

    public bool isMoving { get; private set; } = false;
    public int startFloorIndex { get; private set; } = 0;
    public int nextFloorIndex { get; private set; } = 0;

    private float moveSpeed => ((ElevatorLevelData)ownedBuilding.currentLevelData).ElevatorMoveSpeed;
    private Vector3 moveDirection = Vector3.zero;

    private TimerHandle startMovingTimerHandle = new TimerHandle();
    private const float delayToStartMoving = 1f;

    public ElevatorBuilding ownedElevator => ownedBuilding as ElevatorBuilding;
    public static event System.Action<ElevatorPlatformConstruction> onElevatorPlatformStopped;
    public static event System.Action<ElevatorPlatformConstruction> onElevatorPlatformChangedFloor;

    protected override void OnEnable()
    {
        base.OnEnable();
        Entity.OnEntityStopped += OnEntityStopped;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Entity.OnEntityStopped -= OnEntityStopped;
    }

    protected void Update()
    {
        if (isMoving)
        {
            SetOwnedBuilding(GetFloorIndexByPosition());

            float speed = moveSpeed * Time.deltaTime;
            Move(moveDirection, speed);

            if (floorIndex == nextFloorIndex)
                StopMoving();
        }
    }

    public override void Build(Building ownedBuilding)
    {
        base.Build(ownedBuilding);

        cityManager = FindAnyObjectByType<CityManager>();
    }

    private void StartMovingToFloor(int targetFloorIndex)
    {
        Debug.Log("StartMovingToFloor " + targetFloorIndex);
        if (targetFloorIndex != floorIndex)
        {
            Debug.Log("targetFloorIndex != floorIndex");
            foreach (Entity rider in ridingPassengers) {
                if (rider.isMoving) return;
            }

            isMoving = true;
            startFloorIndex = floorIndex;

            if (targetFloorIndex > floorIndex)
                moveDirection = Vector3.up;
            else if (targetFloorIndex < floorIndex)
                moveDirection = Vector3.down;
        }
    }

    private IEnumerator StartMovingToNextFloorCoroutine()
    {
        yield return new WaitForSeconds(delayToStartMoving);
        StartMovingToFloor(GetNextFloor());
        //TimerManager.SetTimer(startMovingTimerHandle, delayToStartMoving, () => StartMovingToFloor(GetNextFloor()));
    }

    private void StopMoving()
    {
        isMoving = false;

        // Correct position.
        transform.position = new Vector3(transform.position.x, floorIndex * CityManager.floorHeight + CityManager.firstFloorHeight, transform.position.z);

        // Stop entities riding.
        onElevatorPlatformStopped?.Invoke(this);

        // Continue riding to next floor.
        if (ridingPassengers.Count > 0 || waitingPassengers.Count > 0) {
            StartCoroutine(StartMovingToNextFloorCoroutine());
        }
    }

    private void TryToStopMovingToFloor()
    {
        if (isMoving) return;

        //TimerManager.RemoveTimer
    }

    private int GetNextFloor()
    {
        if (ridingPassengers.Count > 0) {
            foreach (Entity rider in ridingPassengers) {
                if (rider.currentPathBuilding) {
                    nextFloorIndex = rider.currentPathBuilding.floorIndex;
                    break;
                }
            }

            if (ridingPassengers.Count < ownedBuilding.currentLevelData.maxResidentsCount && waitingPassengers.Count > 0) {
                foreach (Entity waiter in waitingPassengers) {
                    if (waiter.currentPathBuilding) {
                        if ((nextFloorIndex < floorIndex && waiter.currentBuilding.floorIndex < floorIndex)
                        || (nextFloorIndex > floorIndex && waiter.currentBuilding.floorIndex > floorIndex))
                            nextFloorIndex = waiter.currentBuilding.floorIndex;
                    }
                }
            }
            else {
                foreach (Entity rider in ridingPassengers) {
                    if (rider.currentPathBuilding) {
                        if ((nextFloorIndex < floorIndex && rider.currentPathBuilding.floorIndex < floorIndex && rider.currentPathBuilding.floorIndex > nextFloorIndex)
                        || (nextFloorIndex > floorIndex && rider.currentPathBuilding.floorIndex > floorIndex && rider.currentPathBuilding.floorIndex < nextFloorIndex))
                            nextFloorIndex = rider.currentPathBuilding.floorIndex;
                    }
                }
            }
        }
        else if (waitingPassengers.Count > 0) {
            nextFloorIndex = waitingPassengers[0].floorIndex;
        }
        else {
            nextFloorIndex = floorIndex;
        }

        return nextFloorIndex;
    }

    private void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;

        for (int i = 0; i < ridingPassengers.Count; i++)
            ridingPassengers[i].Move(direction, speed);
    }

    public void AddWaitingPassenger(Entity passenger)
    {
        waitingPassengers.Add(passenger);
    }

    public void RemoveWaitingPassenger(Entity passenger)
    {
        waitingPassengers.Remove(passenger);
    }

    public void AddRidingPassenger(Entity passenger)
    {
        ridingPassengers.Add(passenger);       
    }

    public void RemoveRidingPassenger(Entity passenger)
    {
        ridingPassengers.Remove(passenger);
    }

    private void OnEntityStopped(Entity entity)
    {
        Debug.Log("OnEntityStopped");

        if (entity.isRidingOnElevator && entity.currentElevator == ownedElevator) {
            Debug.Log("1");
            StartCoroutine(StartMovingToNextFloorCoroutine());
        }
        else if (entity.isWaitingForElevator) {
            Debug.Log("2");
            StartMovingToFloor(GetNextFloor());
        }
    }

    public void SetOwnedBuilding(int newFloorIndex)
    {
        if (newFloorIndex != floorIndex && newFloorIndex >= 0) {
            ownedBuilding = cityManager.builtFloors[newFloorIndex].roomBuildingPlaces[placeIndex].placedBuilding;
            onElevatorPlatformChangedFloor?.Invoke(this);
        }
    }

    private int GetFloorIndexByPosition()
    {
        int floorIndex = 0;

        if (nextFloorIndex >= this.floorIndex) {
            floorIndex = (int)((transform.position.y - CityManager.firstFloorHeight) / CityManager.floorHeight);
            if (floorIndex < startFloorIndex)
                floorIndex = startFloorIndex;
        }
        else {
            floorIndex = (int)((transform.position.y - CityManager.firstFloorHeight + CityManager.floorHeight) / CityManager.floorHeight);
            if (floorIndex > startFloorIndex)
                floorIndex = startFloorIndex;
        }
        return floorIndex;
    }
}
