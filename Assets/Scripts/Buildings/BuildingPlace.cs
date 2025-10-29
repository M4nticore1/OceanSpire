using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuildingData;

public enum BuildingPlaceState
{
    Valid,
    Warning,
    Invalid
}

public class BuildingPlace : MonoBehaviour
{
    [HideInInspector] public CityManager cityManager = null;

    public BuildingType buildingType = BuildingType.Room;

    public int floorIndex = 0;
    public int buildingPlaceIndex = 0;
    public int emptyBuildingPlacesAbove = 0;
    public int emptyBuildingPlacesBelow = 0;

    //public bool isBuildingPlaced = false;
    public Building placedBuilding = null;

    [SerializeField] private GameObject buildingZone = null;
    [SerializeField] private GameObject buildingFrame = null;
    [SerializeField] private MeshRenderer buildingZoneMeshRenderer = null;
    [SerializeField] private BoxCollider boxCollider = null;

    private MaterialPropertyBlock materialPropertyBlock = null;
    private MaterialPropertyBlock outlineMaterialPropertyBlock = null;

    private Color buildingPlaceValidColor = new Color(0.2f, 1, 0.2f, 1);
    private Color buildingPlaceWarningColor = new Color(1, 1, 0, 1);
    private Color buildingPlaceInvalidColor = new Color(1, 0, 0, 1);

    private Color buildingPlaceValidOutlineColor = new Color(0.035f, 1, 0, 1);
    private Color buildingPlaceWarningOutlineColor = new Color(1, 1, 0, 1);
    private Color buildingPlaceInvalidOutlineColor = new Color(1, 0, 0, 1);

    public void InitializeBuildingPlace(int newFloorindex)
    {
        cityManager = FindAnyObjectByType<CityManager>();
        buildingZoneMeshRenderer = buildingZone.GetComponent<MeshRenderer>();

        floorIndex = newFloorindex;

        materialPropertyBlock = new MaterialPropertyBlock();
        outlineMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    public void LoadPlacedBuilding()
    {
        if (placedBuilding.isInitialized) return;

        if (placedBuilding)
            PlaceBuilding(placedBuilding, placedBuilding.levelIndex, placedBuilding.isUnderConstruction, -1);
    }

    public void PlaceBuilding(Building buildingToPlace, int levelIndex, bool isUnderConstruction, int interiorIndex)
    {
        if (emptyBuildingPlacesAbove >= buildingToPlace.buildingData.buildingFloors - 1)
        {
            if (!placedBuilding)
            {
                placedBuilding = Instantiate(buildingToPlace, transform.position, transform.rotation);

                if (placedBuilding.buildingData.buildingType == BuildingType.FloorFrame)
                    placedBuilding.transform.SetParent(cityManager.towerRoot);
                else
                    placedBuilding.transform.SetParent(transform);
            }

            placedBuilding.Place(this, levelIndex, isUnderConstruction, interiorIndex);

            if (buildingFrame)
                buildingFrame.SetActive(false);
        }
    }

    public void DestroyBuilding()
    {
        if (placedBuilding)
        {
            boxCollider.enabled = true;

            //isBuildingPlaced = false;
            placedBuilding = null;

            if (buildingFrame)
                buildingFrame.SetActive(true);
        }
    }

    public void AddPlacedBuilding(Building newPlacedBuilding)
    {
        //isBuildingPlaced = true;
        placedBuilding = newPlacedBuilding;

        if (buildingFrame)
            buildingFrame.SetActive(false);
    }

    public void RemoveBuildingPlaced()
    {
        //isBuildingPlaced = false;
        placedBuilding = null;

        if (buildingFrame)
            buildingFrame.SetActive(true);
    }

    public void ShowBuildingPlace(BuildingPlaceState buildingPlaceState)
    {
        buildingZone.SetActive(true);
        boxCollider.enabled = true;

        Color mainColor = Color.black;
        Color outlineColor = Color.black;

        if (buildingPlaceState == BuildingPlaceState.Valid)
        {
            mainColor = buildingPlaceValidColor;
            outlineColor = buildingPlaceValidOutlineColor;
        }
        else if (buildingPlaceState == BuildingPlaceState.Warning)
        {
            mainColor = buildingPlaceWarningColor;
            outlineColor = buildingPlaceWarningOutlineColor;
        }
        else if (buildingPlaceState == BuildingPlaceState.Invalid)
        {
            mainColor = buildingPlaceInvalidColor;
            outlineColor = buildingPlaceInvalidOutlineColor;
        }

        materialPropertyBlock.SetColor("_BaseColor", mainColor);
        buildingZoneMeshRenderer.SetPropertyBlock(materialPropertyBlock, 0);

        outlineMaterialPropertyBlock.SetColor("_OutlineColor", outlineColor);
        buildingZoneMeshRenderer.SetPropertyBlock(outlineMaterialPropertyBlock, 1);
    }

    public void HideBuildingPlace()
    {
        buildingZone.SetActive(false);
        boxCollider.enabled = false;
    }

    public void SetColliderSize(Vector3 NewColliderSize)
    {
        boxCollider.size = NewColliderSize;
        boxCollider.center = new Vector3(0, NewColliderSize.y / 2, 0);
    }
}
