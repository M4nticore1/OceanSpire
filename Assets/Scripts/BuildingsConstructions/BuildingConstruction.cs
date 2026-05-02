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

    public Building ownedBuilding { get; private set; }

    [SerializeField] private BuildingAction[] buildingInteractions;
    public BuildingAction[] BuildingInteractions => buildingInteractions;

    private MeshRenderer[] meshRendererers;
    private MaterialPropertyBlock propertyBlock;

    public virtual void Init(Building ownedBuilding)
    {
        lightProbeGroupManager = FindAnyObjectByType<LightProbeGroupManager>();
        meshRendererers = GetComponentsInChildren<MeshRenderer>();

        foreach (var renderer in meshRendererers) {
            renderer.probeAnchor = lightProbeGroupManager.ProbeAnchor;
        }

        this.ownedBuilding = ownedBuilding;
        propertyBlock = new MaterialPropertyBlock();
    }

    public virtual void SetOwnedBuilding(Building building)
    {
        if (building == ownedBuilding)
            return;

        ownedBuilding = building;
        if (!ownedBuilding)
            return;

        ApplyOwnedBuildingPosition();
    }

    public void ApplyOwnedBuildingPosition()
    {
        transform.position = ownedBuilding.transform.position;
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
        ownedBuilding.OnConstructionClicked();
    }

    public bool ShouldClick()
    {
        return true;
    }
}