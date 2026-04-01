using System;
using UnityEngine;

public enum HumanStatus
{
    Citizen,
    Wanderer,
    Enemy
}

[Serializable]
public class HumanEntry : CreatureEntry
{
    public HumanEntry(int id, HumanStatus status, Vector3 position, Vector3 rotation) : base(id, position, rotation)
    {
        this.status = status;
    }

    public HumanStatus status { get; private set; } = HumanStatus.Citizen;
    public Guid interactBuildingInstanceId { get; private set; } = Guid.Empty;
    public bool isMale { get; private set; } = false;
    public int firstNameIndex { get; private set; } = 0;
    public int lastNameIndex { get; private set; } = 0;
}

public class Human : Creature
{
    [SerializeField] private EntityInteractor interactor;
    public EntityInteractor Interactor => interactor;

    [SerializeField] private BoatRider boatRider;
    public BoatRider BoatRider => boatRider;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    public HumanStatus currentStatus { get; private set; } = HumanStatus.Citizen;

    private bool isMale = false;

    private int firstNameIndex = 0;
    private int lastNameIndex = 0;

    public string firstName { get; private set; }
    public string lastName { get; private set; }

    public static event Action<Human> onWandererAccepted;
    public static event Action<Human> onWandererRejected;
    public static event Action<Human> onHumanSelected;
    public static event Action<Human> onHumanDeselected;

    protected override void OnEnable()
    {
        base.OnEnable();

        movement.onStoppedMoving += OnStoppedMoving;

        cityNavigator.onEnteredBuilding += OnEnteredBuilding;
        cityNavigator.onExitedBuilding += OnExitedBuilding;
        cityNavigator.onReachedTarget += OnReachedTargetBuilding;

        interactor.onSetedInteractBuilding += OnSetedInteractBuilding;
        interactor.onRemovedInteractBuilding += OnRemovedInteractBuilding;
        interactor.onStoppedInteracting += OnStopInteracting;

        boatRider.onEnteredBoat += OnEnteredBoat;
        boatRider.onExitedBoat += OnExitedBoat;

        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        cityNavigator.onEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.onExitedBuilding -= OnExitedBuilding;

        interactor.onSetedInteractBuilding -= OnSetedInteractBuilding;
        interactor.onRemovedInteractBuilding -= OnRemovedInteractBuilding;
        interactor.onStoppedInteracting -= OnStopInteracting;

        boatRider.onEnteredBoat -= OnEnteredBoat;
        boatRider.onExitedBoat -= OnExitedBoat;

        selectComponent.onSelected -= OnSelected;
        selectComponent.onDeselected -= OnDeselected;
    }

    public override void Init(CreatureEntry data)
    {
        base.Init(data);

        HumanEntry humanData = data as HumanEntry;
        SetStatus(humanData.status);
        isMale = humanData.isMale;
        AssignNameIndexes(humanData);

        EventBus.InvokeCitizenInited(this);
    }

    // Wanderer
    public void AcceptWanderer()
    {
        SetStatus(HumanStatus.Citizen);
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
        if (interactor.InteractBuilding && interactor.InteractBuilding == cityNavigator.currentBuilding) {
            interactor.HandleStoppedMoving();
        }
    }

    private void OnEnteredBuilding(Building building)
    {
        building.EnterBuilding(cityNavigator);
    }

    private void OnExitedBuilding(Building building)
    {
        building.ExitBuilding(cityNavigator);
    }

    private void OnSetedInteractBuilding(Building building)
    {
        cityNavigator.SetTargetBuilding(building);

        if (boatRider.isRidingOnBoat)
            return;

        cityNavigator.TryFindPathToTargetBuilding();
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

        if (currentStatus == HumanStatus.Citizen) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
        else if (currentStatus == HumanStatus.Wanderer) {
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
    public void SetStatus(HumanStatus status)
    {
        ExitStatus(currentStatus);
        currentStatus = status;
        EnterStatus(currentStatus);
    }

    private void EnterStatus(HumanStatus status)
    {
        switch (status) {
            case HumanStatus.Citizen:
                CreaturesManager.instance.RegisterCitizen(this);
                break;
            case HumanStatus.Wanderer:
                CreaturesManager.instance.RegisterWanderer(this);
                selectComponent.SetClickable(false);
                break;
            case HumanStatus.Enemy:
                CreaturesManager.instance.RegisterEnemy(this);
                break;
        }
    }

    private void ExitStatus(HumanStatus status)
    {
        switch (status) {
            case HumanStatus.Citizen:
                CreaturesManager.instance.UnregisterCitizen(this);
                break;
            case HumanStatus.Wanderer:
                CreaturesManager.instance.UnregisterWanderer(this);
                selectComponent.SetClickable(true);
                break;
            case HumanStatus.Enemy:
                CreaturesManager.instance.UnregisterEnemy(this);
                break;
        }
    }

    private void OnSelected()
    {
        if (currentStatus == HumanStatus.Wanderer) {
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
        if (currentStatus == HumanStatus.Wanderer) {
            SelectComponent boatSelectComponent = boatRider.currentBoat.SelectComponent;
            boatSelectComponent.SetClickable(true);
            boatSelectComponent.SetSelected(false);
        }
        else {
            onHumanDeselected?.Invoke(this);
        }
    }
}