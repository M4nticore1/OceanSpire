using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
    private GameManager gameManager = null;
    private PlayerController playerController = null;
    private UIManager UIManager = null;

    private ConstructionComponent constructionComponent = null;
    [SerializeField] private BuildingResourceWidget buildingResourceWidget = null;
    private List<BuildingResourceWidget> spawnedBuildingResourceWidgets = new List<BuildingResourceWidget>();

    [SerializeField] private Image buildingImage = null;
    [SerializeField] private MainButton buildButton = null;
    [SerializeField] private MainButton informationButton = null;

    [SerializeField] private TextMeshProUGUI buildingNameText = null;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup = null;
    //[SerializeField] private HorizontalLayoutGroup resourcesToBuildHorizontalLayoutGroupWidget = null;

    public static event Action<ConstructionComponent> OnStartPlacingConstruction;

    int resourcesToBuildNumber = 0;

    private void OnEnable()
    {
        //buildButton.onPress += () => OnPress?.Invoke();
        //openInformationButton.onPress += () => OnPress?.Invoke();
        //buildButton.onRelease += () => OnRelease?.Invoke();
        //openInformationButton.onRelease += () => OnRelease?.Invoke();
    }

    private void OnDisable()
    {
        //buildButton.onPress -= () => OnPress?.Invoke();
        //openInformationButton.onPress -= () => OnPress?.Invoke();
        //buildButton.onRelease -= () => OnRelease?.Invoke();
        //openInformationButton.onRelease -= () => OnRelease?.Invoke();
    }

    public void InitializeBuildingWidget(GameManager gameManager, ConstructionComponent construction)
    {
        this.gameManager = gameManager;
        constructionComponent = construction;

        playerController = GetComponentInParent<PlayerController>();
        UIManager = playerController.GetComponentInChildren<UIManager>();

        buildButton.onClick.AddListener(StartPlacingBuilding);
        informationButton.onClick.AddListener(OpenBuildingInformationMenu);

        Building building = construction.GetComponentInChildren<Building>();
        if (building) {
            buildingNameText.SetText(building.BuildingData.BuildingName);

            if (building.ConstructionLevelsData.Count >= 1 && building.ConstructionLevelsData[0])
                resourcesToBuildNumber = building.ConstructionLevelsData[0].ResourcesToBuild.Count();
            else
                Debug.LogWarning($"{building.BuildingData.BuildingName} has no LevelData by index 0 or has not instance");

            if (building.BuildingData.ThumbImage)
                buildingImage.sprite = building.BuildingData.ThumbImage;
        }
        DrawResourcesToBuild();
    }

    private void DrawResourcesToBuild()
    {
        Building building = constructionComponent.GetComponentInChildren<Building>();

        for (int i = 0; i < resourcesToBuildNumber; i++)
        {
            if(!constructionComponent)
                Debug.Log("building is NULL");
            if (!buildingResourceWidget)
                Debug.Log("buildingResourceWidget is NULL");
            if (building && !building.ConstructionLevelsData[0])
                Debug.Log("building.buildingLevelsData[0] is NULL");
            //if (building.buildingLevelsData[0].ResourcesToBuild[i])
                //Debug.Log("building.buildingLevelsData[0].ResourcesToBuild[i] is NULL");

            BuildingResourceWidget spawnedBuildingResourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);
            spawnedBuildingResourceWidgets.Add(spawnedBuildingResourceWidget);

            //spawnedBuildingResourceWidgets[i].Initialize(building.buildingLevelsData[0].resourcesToBuild[i].Amount, building.buildingLevelsData[0].resourcesToBuild[i].ItemData.itemIcon);
        }
    }

    private void StartPlacingBuilding()
    {
        //List<bool> haveResourcesToBuild = new List<bool>();

        //for (int i = 0; i < resourcesToBuildNumber; i++)
        //{
        //    int id = building.ConstructionLevelsData[0].ResourcesToBuild[i].ItemData.ItemId;

        //    if (cityManager.items[id].Amount >= building.ConstructionLevelsData[0].ResourcesToBuild[i].Amount)
        //        haveResourcesToBuild.Add(true);
        //    else
        //        haveResourcesToBuild.Add(false);
        //}

        //if (!haveResourcesToBuild.Contains(false) || resourcesToBuildNumber == 0)
        //{
        //    playerController.StartPlacingBuilding(building.constructionComponent);

        //    UIManager.CloseManagementMenu();
        //}

        OnStartPlacingConstruction?.Invoke(constructionComponent);
    }

    private void OpenBuildingInformationMenu()
    {
        UIManager.OpenBuildingInformationMenu(constructionComponent);
    }

    public void UpdateResourcesToBuild()
    {
        Building building = constructionComponent.GetComponentInChildren<Building>();
        for (int i = 0; i < resourcesToBuildNumber; i++)
        {
            int id = building.ConstructionLevelsData[0].ResourcesToBuild[i].ItemData.ItemId;

            spawnedBuildingResourceWidgets[i].SetResourceText(gameManager.items[id].Amount, building.ConstructionLevelsData[0].ResourcesToBuild[i].Amount);
        }
    }
}
