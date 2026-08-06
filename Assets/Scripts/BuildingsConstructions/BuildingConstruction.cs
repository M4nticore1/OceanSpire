using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BuildingAction
{
    [SerializeField] private BuildingActionWaypoint[] waypoints;
    public BuildingActionWaypoint[] Waypoints => waypoints;

    public BuildingActionWaypoint GetWaypoint(int index)
    {
        if (index >= waypoints.Length) {
            Debug.LogError("index is over than waypoints length");
            return null;
        }

        return waypoints[index];
    }
}

[System.Serializable]
public class BuildingActionWaypoint
{
    [SerializeField] private Transform transform;
    public Transform Transform => transform;

    [SerializeField] private int actionTime;
    public int ActionTime => actionTime;
}

public class BuildingConstruction : MonoBehaviour, IClickable
{
    private LightProbeGroupManager lightProbeGroupManager;

    public Building OwnedBuilding;

    [SerializeField] private BuildingAction[] buildingInteractions;
    public BuildingAction[] BuildingInteractions => buildingInteractions;

    private MeshRenderer[] meshRendererers;
    private MaterialPropertyBlock propertyBlock;

    private Dictionary<CreatureCityNavigator, BuildingAction> interactionsDict = new();
    public IReadOnlyDictionary<CreatureCityNavigator, BuildingAction> InteractionsDict => interactionsDict;

    private List<BuildingAction> interactionsList = new();
    public IReadOnlyList<BuildingAction> InteractionsList => interactionsList;

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

    public static event Action<BuildingConstruction> OnBuildingConstructionInited;
    public static event Action<BuildingConstruction> OnBuildingConstructionDemolished;

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void Awake()
    {
        lightProbeGroupManager = FindAnyObjectByType<LightProbeGroupManager>();
        meshRendererers = GetComponentsInChildren<MeshRenderer>();

        foreach (var renderer in meshRendererers) {
            renderer.probeAnchor = lightProbeGroupManager.ProbeAnchor;
        }

        propertyBlock = new MaterialPropertyBlock();
    }

    public void Init(BuildingConstructionData data)
    {
        OnInited(data);
        OnBuildingConstructionInited?.Invoke(this);
    }

    protected virtual void OnInited(BuildingConstructionData data)
    {
        if (data == null) {
            Debug.LogError($"[{nameof(BuildingConstruction)}] BuildingConstruction is not vaid!");
            return;
        }

        var building = InstancesManager.Instance.GetInstance(data.OwnedBuildingInstanceId).GetComponent<Building>();
        if (building == null) {
            Debug.LogError($"[{nameof(BuildingConstruction)}] Instance Id is not building!");
            return;
        }

        SetOwnedBuilding(building);

        UpdateWorkerInteractionTransforms();
        UpdateRaiderInteractionTransforms();
    }

    public virtual void SetOwnedBuilding(Building building)
    {
        OwnedBuilding = building;
    }

    public void Demolish()
    {
        OnDemolished();
        Destroy(gameObject);

        OnBuildingConstructionDemolished?.Invoke(this);
    }

    protected virtual void OnDemolished()
    {

    }

    public void ApplyConstructionPosition()
    {
        transform.position = OwnedBuilding.transform.position;
    }

    public void SetFlickingPower(float power)
    {
        propertyBlock.SetFloat("_FlickingPower", power);

        foreach (var renderer in meshRendererers) {
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    // Interaction
    public void AssignInteract(CreatureCityNavigator navigator)
    {
        if (interactionsDict.ContainsKey(navigator))
            return;

        interactionsDict.Add(navigator, GetInteractPoint(interactionsDict.Count));
    }

    public void RemoveInteract(CreatureCityNavigator navigator)
    {
        if (!interactionsDict.ContainsKey(navigator))
            return;

        interactionsDict.Remove(navigator);
    }

    public void UpdateWorkerInteractionTransforms()
    {
        for (int i = 0; i < OwnedBuilding.CitizensHandler.Interactors.Count; i++) {
            var worker = OwnedBuilding.CitizensHandler.Interactors[i];
            var navigator = worker.CityNavigator;

            AssignInteract(navigator);
        }
    }

    public void UpdateRaiderInteractionTransforms()
    {
        for (int i = 0; i < OwnedBuilding.RaidersHandler.Interactors.Count; i++) {
            var raider = OwnedBuilding.RaidersHandler.Interactors[i];
            if (!raider) continue;

            var navigator = raider.CityNavigator;

            AssignInteract(navigator);
        }
    }

    public void UpdateInteractTransforms()
    {
        interactionsList.Clear();

        var keys = interactionsDict.Keys.ToArray();
        for (int i = 0; i < keys.Length; i++) {
            if (i >= BuildingInteractions.Length) break;

            var interaction = BuildingInteractions[i];
            interactionsDict[keys[i]] = interaction;
            interactionsList.Add(interaction);
        }
    }

    public BuildingAction GetInteractPoint(int index)
    {
        var actions = BuildingInteractions;

        if (actions.Length <= 0) {
            Debug.LogError($"[{nameof(BuildingConstruction)}] Intreactions count is 0 at {name}!");
            return null;
        }

        index %= actions.Length;
        return actions[index];
    }

    public BuildingAction GetInteractPoint(CreatureCityNavigator navigator)
    {
        if (!interactionsDict.ContainsKey(navigator))
            return null;

        return interactionsDict[navigator];
    }

    // IClickable
    public void Click()
    {
        OwnedBuilding.OnConstructionClicked();
        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        IsClickable = value;
    }

    public bool ShouldClick()
    {
        return IsClickable;
    }
}