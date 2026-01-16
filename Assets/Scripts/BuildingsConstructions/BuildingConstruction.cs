using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct BuildingAction
{
    public Transform[] waypoints;
    public int[] actionTimes;
}

public class BuildingConstruction : MonoBehaviour
{
    protected GameManager gameManager;
    protected Building ownedBuilding = null;

    [SerializeField] private GameObject[] buildingInteriors;
    public GameObject[] BuildingInteriors => buildingInteriors;

    [SerializeField] private BuildingAction[] buildingInteractions;
    public BuildingAction[] BuildingInteractions => buildingInteractions;

    [Header("Storage")]
    public List<Transform> collectItemPoints = new List<Transform>();

    private MeshRenderer[] meshRendererers = null;
    private MaterialPropertyBlock propertyBlock = null;

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    public virtual void Build(GameManager gameManager, Building ownedBuilding)
    {
        this.gameManager = gameManager;
        this.ownedBuilding = ownedBuilding;
        meshRendererers = GetComponentsInChildren<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetFlickingMultiplier(float multiplier)
    {
        Debug.Log("SetFlickingMultiplier " + multiplier);
        propertyBlock.SetFloat("_FlickingMultiplier", multiplier);
        foreach (MeshRenderer renderer in meshRendererers)
            renderer.SetPropertyBlock(propertyBlock);
    }
}
