using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class BuildingComponent : MonoBehaviour
{
    protected GameManager gameManager = null;
    protected CityManager cityManager = null;
    [HideInInspector] public Building ownedBuilding = null;

    public BuildingComponentLevelData[] levelsData = new BuildingComponentLevelData[0];

    protected virtual void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        ownedBuilding = GetComponent<Building>();
    }

    protected virtual void OnEnable()
    {
        //ownedBuilding.onBuildingStartConstructing += Build;
        //ownedBuilding.onBuildingFinishConstructing += Build;
    }

    protected virtual void OnDisable()
    {
        //ownedBuilding.onBuildingStartConstructing -= Build;
        //ownedBuilding.onBuildingFinishConstructing -= Build;
    }

    public virtual void Build(int newLevel)
    {

    }

    //public virtual void UpdateLevel(int newLevel)
    //{

    //}
}
