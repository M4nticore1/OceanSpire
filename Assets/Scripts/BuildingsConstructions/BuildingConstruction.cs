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

public class BuildingConstruction : MonoBehaviour
{
    [SerializeField] private SelectComponent selectComponent;

    private LightProbeGroupManager lightProbeGroupManager;

    public Building ownedBuilding { get; private set; } = null;

    [SerializeField] private GameObject[] buildingInteriors;
    public GameObject[] BuildingInteriors => buildingInteriors;

    [SerializeField] private BuildingAction[] buildingInteractions;
    public BuildingAction[] BuildingInteractions => buildingInteractions;

    private MeshRenderer[] meshRendererers = null;
    private MaterialPropertyBlock propertyBlock = null;

    protected virtual void OnEnable()
    {
        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;
    }

    protected virtual void OnDisable()
    {
        selectComponent.onSelected -= OnSelected;
        selectComponent.onDeselected -= OnDeselected;
    }

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

    public void Click()
    {

    }

    public bool CanClick()
    {
        return true;
    }

    private void OnSelected()
    {
        ownedBuilding.OnSelected();
    }

    private void OnDeselected()
    {
        ownedBuilding.OnDeselected();
    }
}