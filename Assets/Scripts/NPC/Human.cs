using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum HumanStateEnum
{
    Citizen,
    Wanderer,
    Raider
}

[Serializable]
public class HumanEntry : CreatureEntry
{
    public HumanEntry(int id, HumanStateEnum status, Vector3 position, Vector3 rotation) : base(id, position, rotation)
    {
        this.state = status;
    }

    public HumanStateEnum state { get; private set; } = HumanStateEnum.Citizen;

    public Guid interactBuildingInstanceId { get; private set; } = Guid.Empty;
    public bool isMale { get; private set; } = false;
    public int firstNameIndex { get; private set; } = 0;
    public int lastNameIndex { get; private set; } = 0;
}

public class Human : Creature
{
    [SerializeField] private EntityCityNavigator cityNavigator;
    public EntityCityNavigator CityNavigator => cityNavigator;

    [SerializeField] private EntityInteractor interactor;
    public EntityInteractor Interactor => interactor;

    [SerializeField] private BoatRider boatRider;
    public BoatRider BoatRider => boatRider;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    public HumanStateEnum currentStateEnum { get; private set; } = HumanStateEnum.Citizen;
    public HumanState currentState { get; private set; }

    private bool isMale = false;

    private int firstNameIndex = 0;
    private int lastNameIndex = 0;

    public string firstName { get; private set; }
    public string lastName { get; private set; }

    public static event Action<Human> onWandererAccepted;
    public static event Action<Human> onWandererRejected;
    public static event Action<Human> onHumanSelected;
    public static event Action<Human> onHumanDeselected;

    protected void OnEnable()
    {
        movement.onStoppedMoving += OnStoppedMoving;

        cityNavigator.onEnteredBuilding += OnEnteredBuilding;
        cityNavigator.onExitedBuilding += OnExitedBuilding;
        cityNavigator.onReachedTarget += OnReachedTargetBuilding;

        interactor.onRemovedInteractBuilding += OnRemovedInteractBuilding;
        interactor.onStoppedInteracting += OnStopInteracting;

        boatRider.onEnteredBoat += OnEnteredBoat;
        boatRider.onExitedBoat += OnExitedBoat;

        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;

        EventBus.onNavMeshBaked += OnNavMeshBaked;
    }

    protected void OnDisable()
    {
        cityNavigator.onEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.onExitedBuilding -= OnExitedBuilding;

        interactor.onRemovedInteractBuilding -= OnRemovedInteractBuilding;
        interactor.onStoppedInteracting -= OnStopInteracting;

        boatRider.onEnteredBoat -= OnEnteredBoat;
        boatRider.onExitedBoat -= OnExitedBoat;

        selectComponent.onSelected -= OnSelected;
        selectComponent.onDeselected -= OnDeselected;

        EventBus.onNavMeshBaked += OnNavMeshBaked;
    }

    public override void Init(CreatureEntry data)
    {
        base.Init(data);

        HumanEntry humanData = data as HumanEntry;
        SetStatus(humanData.state);
        isMale = humanData.isMale;
        AssignNameIndexes(humanData);

        EventBus.InvokeCitizenInited(this);
    }

    public void SetInteractBuilding(Building building)
    {
        if (interactor.InteractBuilding) {
            RemoveInteractBuilding();
        }

        interactor.SetInteractBuilding(building);
        currentState.OnSetedInteractBuilding(building);
        cityNavigator.SetTargetBuilding(building);

        if (boatRider.isRidingOnBoat) {
            BoatRider.currentBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            cityNavigator.TryFindPathToTargetBuilding();
        }
    }

    public void RemoveInteractBuilding()
    {
        currentState.OnRemovedInteractBuilding();
        cityNavigator.RemoveTargetBuilding();

        if (boatRider.isRidingOnBoat) {
            BoatRider.currentBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            movement.StopMoving();
        }
    }

    public void HandleClickedWorkerWidget()
    {
        if (interactor.InteractBuilding) {
            RemoveInteractBuilding();
        }
        else {
            Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();
            SetInteractBuilding(building);
        }
    }

    // Wanderer
    public void AcceptWanderer()
    {
        SetStatus(HumanStateEnum.Citizen);
        selectComponent.SetClickable(true);
        selectComponent.SetSelected(false);
        Destroy(boatRider.currentBoat.gameObject);
        boatRider.ExitBoat();
        onWandererAccepted?.Invoke(this);
    }

    public void RejectWanderer()
    {
        boatRider.currentBoat.SetState(BoatStateEnum.FloatingAway);
        CreaturesManager.instance.UnregisterWanderer(this);
        onWandererRejected?.Invoke(this);
    }

    // Movement
    private void OnStoppedMoving()
    {
        currentState.OnStoppedMoving();
    }

    private void OnEnteredBuilding(Building building)
    {
        building.EnterBuilding(cityNavigator);
    }

    private void OnExitedBuilding(Building building)
    {
        building.ExitBuilding(cityNavigator);
    }

    private void OnRemovedInteractBuilding(Building building)
    {
        cityNavigator.HandleInteractBuildingRemoved();
    }

    private void OnReachedTargetBuilding(Building building)
    {
        //interactor.OnReachedTargetBuilding();
    }

    private void OnStopInteracting(Building building)
    {

    }

    private void OnEnteredBoat(Boat boat)
    {
        movement.SetAgentEnabled(false);

        if (currentStateEnum == HumanStateEnum.Citizen) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
        else if (currentStateEnum == HumanStateEnum.Wanderer) {
            boat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    private void OnExitedBoat(Boat boat)
    {
        movement.SetAgentEnabled(true);

        if (!interactor.InteractBuilding)
            return;

        cityNavigator.TryFindPathToTargetBuilding();
    }

    // Names
    private void AssignNameIndexes(HumanEntry data)
    {
        if (data != null) {
            firstNameIndex = data.firstNameIndex;
            lastNameIndex = data.lastNameIndex;
        }
        else {
            if (isMale) {
                //firstNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.male_first_names.Length);
                //lastNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.male_last_names.Length);
            }
            else {
                //firstNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.female_first_names.Length);
                //lastNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.female_last_names.Length);
            }
            firstNameIndex = 0;
            lastNameIndex = 0;
        }

        firstName = LocalizationManager.Instance.GetFirstName(isMale, firstNameIndex);
        lastName = LocalizationManager.Instance.GetLastName(isMale, lastNameIndex);
    }

    // Status
    public void SetStatus(HumanStateEnum status)
    {
        ExitStatus(currentStateEnum);
        currentStateEnum = status;
        EnterStatus(currentStateEnum);
    }

    private void EnterStatus(HumanStateEnum status)
    {
        switch (status) {
            case HumanStateEnum.Citizen:
                currentState = new CitizenState(this);
                CreaturesManager.instance.RegisterCitizen(this);
                break;
            case HumanStateEnum.Wanderer:
                currentState = new WandererState(this);
                CreaturesManager.instance.RegisterWanderer(this);
                selectComponent.SetClickable(false);
                break;
            case HumanStateEnum.Raider:
                currentState = new RaiderState(this);
                CreaturesManager.instance.RegisterEnemy(this);
                break;
        }
    }

    private void ExitStatus(HumanStateEnum status)
    {
        switch (status) {
            case HumanStateEnum.Citizen:
                CreaturesManager.instance.UnregisterCitizen(this);
                break;
            case HumanStateEnum.Wanderer:
                CreaturesManager.instance.UnregisterWanderer(this);
                selectComponent.SetClickable(true);
                break;
            case HumanStateEnum.Raider:
                CreaturesManager.instance.UnregisterEnemy(this);
                break;
        }
    }

    private void OnSelected()
    {
        if (currentStateEnum == HumanStateEnum.Wanderer) {
            selectComponent.SetSelected(false);
            SelectComponent boatSelectComponent = boatRider.currentBoat.SelectComponent;
            boatSelectComponent.SetSelected(true);
            boatSelectComponent.SetClickable(false);
        }
        else {
            onHumanSelected?.Invoke(this);
        }
    }

    private void OnDeselected()
    {
        if (currentStateEnum == HumanStateEnum.Wanderer) {
            SelectComponent boatSelectComponent = boatRider.currentBoat.SelectComponent;
            boatSelectComponent.SetClickable(true);
            boatSelectComponent.SetSelected(false);
        }
        else {
            onHumanDeselected?.Invoke(this);
        }
    }

    private void OnNavMeshBaked()
    {
        if (cityNavigator.IsRidingOnElevator) return;

        movement.SetAgentEnabled(true);
    }
}