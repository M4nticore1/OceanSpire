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
        var building = InstancesManager.Instance.GetInstance(data.BuildingInstanceId).GetComponent<Building>();
        SetOwnedBuilding(building);
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

        foreach (MeshRenderer renderer in meshRendererers) {
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    // Interaction
    public void AssignInteract(CreatureCityNavigator navigator)
    {
        if (interactionsDict.ContainsKey(navigator))
            return;

        interactionsDict.Add(navigator, GetInteraction(interactionsDict.Count));
    }

    public void RemoveInteract(CreatureCityNavigator navigator)
    {
        if (!interactionsDict.ContainsKey(navigator))
            return;

        interactionsDict.Remove(navigator);
    }

    public void UpdateWorkerInteractionTransforms()
    {
        for (int i = 0; i < OwnedBuilding.WorkComponent.Workers.Count; i++) {
            var worker = OwnedBuilding.WorkComponent.Workers[i];
            var navigator = worker.CityNavigator;

            AssignInteract(navigator);
        }
    }

    public void UpdateRaiderInteractionTransforms()
    {
        for (int i = 0; i < OwnedBuilding.RaidComponent.Raiders.Count; i++) {
            var raider = OwnedBuilding.RaidComponent.Raiders[i];
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

    public BuildingAction GetInteraction(CreatureCityNavigator navigator)
    {
        if (!interactionsDict.ContainsKey(navigator))
            return null;

        return interactionsDict[navigator];
    }

    public BuildingAction GetInteraction(int index)
    {
        var actions = BuildingInteractions;
        index %= actions.Length;

        return actions[index];
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