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

    public Building OwnedBuilding { get; private set; }

    [SerializeField] private BuildingAction[] buildingInteractions;
    public BuildingAction[] BuildingInteractions => buildingInteractions;

    private MeshRenderer[] meshRendererers;
    private MaterialPropertyBlock propertyBlock;

    public static event Action<BuildingConstruction> OnBuildingConstructionInited;

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

    public virtual void Demolish()
    {
        Destroy(gameObject);
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
    }

    public bool ShouldClick()
    {
        return true;
    }
}