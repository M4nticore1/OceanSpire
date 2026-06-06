using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingAction
{
    public BuildingActionWaypoint[] waypoints;
}

[System.Serializable]
public class BuildingActionWaypoint
{
    public Transform transform;
    public int actionTime;
}

public class BuildingConstruction : MonoBehaviour, IClickable
{
    private LightProbeGroupManager lightProbeGroupManager;

    public Building OwnedBuilding;

    [SerializeField] private BuildingAction[] buildingInteractions;
    public BuildingAction[] BuildingInteractions => buildingInteractions;

    private MeshRenderer[] meshRendererers;
    private MaterialPropertyBlock propertyBlock;

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

    public static event Action<BuildingConstruction> OnBuildingConstructionInited;
    public static event Action<BuildingConstruction> OnBuildingConstructionDemolished;

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

    public void ApplyBuildingPosition()
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