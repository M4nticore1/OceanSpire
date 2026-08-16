using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BuildingAction
{
    [SerializeField] private InteractionWaypoint[] waypoints;
    public InteractionWaypoint[] Waypoints => waypoints;

    public InteractionWaypoint GetWaypoint(int index)
    {
        if (index >= waypoints.Length) {
            Debug.LogError($"Index is over than waypoints length");
            return null;
        }

        var waypoint = waypoints[index];
        if (waypoint == null) {
            Debug.LogError($"Waypoint is not valid at index {index}");
        }

        return waypoint;
    }
}

[System.Serializable]
public class InteractionWaypoint
{
    [SerializeField] private Transform transform;
    public Transform Transform => transform;

    [SerializeField] private int actionTime;
    public int ActionTime => actionTime;

    [SerializeField] private AnimationParam actionAnimation;
    public AnimationParam ActionAnimation => actionAnimation;
}

public class BuildingConstruction : MonoBehaviour, IClickable
{
    private LightProbeGroupManager lightProbeGroupManager;

    public Building OwnedBuilding;

    [SerializeField] private BuildingAction[] buildingInteractions;
    public BuildingAction[] BuildingInteractions => buildingInteractions;

    [SerializeField] private ConstructionInteractionPointsHandler interactionPointsHandler;
    public ConstructionInteractionPointsHandler InteractionPointsHandler => interactionPointsHandler != null ? interactionPointsHandler : GetComponent<ConstructionInteractionPointsHandler>();

    private MeshRenderer[] meshRendererers;
    private MaterialPropertyBlock propertyBlock;

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

    public void Init()
    {
        Init(BuildingConstructionData.Default());
    }

    public void Init(BuildingConstructionData data)
    {
        if (data == null) {
            Debug.LogError($"[{nameof(BuildingConstruction)}] Building Construction Data is not vaid!");
            Init();
            return;
        }

        HandleInited(data);
        StartCoroutine(InitCoroutine());
        OnBuildingConstructionInited?.Invoke(this);
    }

    protected virtual void HandleInited(BuildingConstructionData data)
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
        interactionPointsHandler.Init();
    }

    protected virtual void HandleInitedEndOfFrame()
    {

    }

    public virtual void SetOwnedBuilding(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(BuildingConstruction)}] Owned building is not valid!!");
            return;
        }

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

    private IEnumerator InitCoroutine()
    {
        yield return new WaitForEndOfFrame();

        HandleInitedEndOfFrame();
    }
}