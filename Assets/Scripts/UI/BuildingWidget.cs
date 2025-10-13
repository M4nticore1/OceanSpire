using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingWidget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public System.Action OnPress;
    public System.Action OnRelease;

    [HideInInspector] public GameManager gameManager = null;
    [HideInInspector] public CityManager cityManager = null;
    private PlayerController playerController = null;
    private UIManager UIManager = null;

    private Building building = null;
    [SerializeField] private BuildingResourceWidget buildingResourceWidget = null;
    private List<BuildingResourceWidget> spawnedBuildingResourceWidgets = new List<BuildingResourceWidget>();

    [SerializeField] private Image buildingImage = null;
    public Button buildButton = null;
    public Button buildingInformationButton = null;
    private bool isInformationOpened = false;

    [SerializeField] private TextMeshProUGUI buildingNameText = null;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup = null;
    //[SerializeField] private HorizontalLayoutGroup resourcesToBuildHorizontalLayoutGroupWidget = null;

    int resourcesToBuildNumber = 0;

    public void InitializeBuildingWidget(Building newBuilding)
    {
        gameManager = FindAnyObjectByType<GameManager>();

        building = newBuilding;

        playerController = GetComponentInParent<PlayerController>();
        UIManager = playerController.GetComponentInChildren<UIManager>();

        buildButton.onClick.AddListener(StartPlacingBuilding);
        buildingInformationButton.onClick.AddListener(ToggleBuildingInformation);

        buildingNameText.SetText(building.buildingData.buildingName);

        if (building.buildingLevelsData.Count() > 0 && building.buildingLevelsData[0])
            resourcesToBuildNumber = building.buildingLevelsData[0].ResourcesToBuild.Count();
        else
            Debug.Log("building.buildingLevelsData[0] is NULL " + building.buildingData.buildingIdName);

        if (newBuilding.buildingData.thumbImage)
            buildingImage.sprite = newBuilding.buildingData.thumbImage;
        DrawResourcesToBuild();
    }

    private void DrawResourcesToBuild()
    {
        for (int i = 0; i < resourcesToBuildNumber; i++)
        {
            if(!building)
                Debug.Log("building is NULL");
            if (!buildingResourceWidget)
                Debug.Log("buildingResourceWidget is NULL");
            if (!building.buildingLevelsData[0])
                Debug.Log("building.buildingLevelsData[0] is NULL");
            //if (building.buildingLevelsData[0].ResourcesToBuild[i])
                //Debug.Log("building.buildingLevelsData[0].ResourcesToBuild[i] is NULL");

            BuildingResourceWidget spawnedBuildingResourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);
            spawnedBuildingResourceWidgets.Add(spawnedBuildingResourceWidget);

            spawnedBuildingResourceWidgets[i].Initialize(building.buildingLevelsData[0].ResourcesToBuild[i].amount, building.buildingLevelsData[0].ResourcesToBuild[i].resourceData.itemIcon);
        }
    }

    private void StartPlacingBuilding()
    {
        List<bool> haveResourcesToBuild = new List<bool>();

        for (int i = 0; i < resourcesToBuildNumber; i++)
        {
            int index = gameManager.GetItemIndexByIdName(building.buildingLevelsData[0].ResourcesToBuild[i].resourceData.itemIdName);

            if (cityManager.items[index].amount >= building.buildingLevelsData[0].ResourcesToBuild[i].amount)
                haveResourcesToBuild.Add(true);
            else
                haveResourcesToBuild.Add(false);
        }

        if (!haveResourcesToBuild.Contains(false) || resourcesToBuildNumber == 0)
        {
            playerController.StartPlacingBuilding(building);

            UIManager.CloseManagementMenu();
        }
    }

    private void ToggleBuildingInformation()
    {
        if (isInformationOpened)
        {
            CloseBuildingInformation();
        }
        else
        {
            OpenBuildingInformation();
        }
    }

    private void OpenBuildingInformation()
    {
        isInformationOpened = true;
    }

    private void CloseBuildingInformation()
    {
        isInformationOpened = false;
    }

    public void UpdateResourcesToBuild()
    {
        for (int i = 0; i < resourcesToBuildNumber; i++)
        {
            int index = gameManager.GetItemIndexByIdName(building.buildingLevelsData[0].ResourcesToBuild[i].resourceData.itemIdName);

            spawnedBuildingResourceWidgets[i].SetResourceText(cityManager.items[index].amount, building.buildingLevelsData[0].ResourcesToBuild[i].amount);
        }
    }

    public void onpoin
}
