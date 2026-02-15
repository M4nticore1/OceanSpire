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
    protected Building ownedBuilding = null;

    [SerializeField] private GameObject[] buildingInteriors;
    public GameObject[] BuildingInteriors => buildingInteriors;

    [SerializeField] private BuildingAction[] buildingInteractions;
    public BuildingAction[] BuildingInteractions => buildingInteractions;

    [Header("Storage")]
    public List<Transform> collectItemPoints = new List<Transform>();

    private MeshRenderer[] meshRendererers = null;
    private MaterialPropertyBlock propertyBlock = null;

    public virtual void Init(Building ownedBuilding)
    {
        this.ownedBuilding = ownedBuilding;
        meshRendererers = GetComponentsInChildren<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetFlickingMultiplier(float multiplier)
    {
        propertyBlock.SetFloat("_FlickingMultiplier", multiplier);
        foreach (MeshRenderer renderer in meshRendererers)
            renderer.SetPropertyBlock(propertyBlock);
    }
}
