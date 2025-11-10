using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
    //public System.Action OnPress;
    //public System.Action OnRelease;

    [HideInInspector] public CityManager cityManager = null;
    private PlayerController playerController = null;
    private UIManager UIManager = null;

    private Building building = null;
    [SerializeField] private BuildingResourceWidget buildingResourceWidget = null;
    private List<BuildingResourceWidget> spawnedBuildingResourceWidgets = new List<BuildingResourceWidget>();

    [SerializeField] private Image buildingImage = null;
    [SerializeField] private MainButton buildButton = null;
    [SerializeField] private MainButton informationButton = null;

    [SerializeField] private TextMeshProUGUI buildingNameText = null;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup = null;
    //[SerializeField] private HorizontalLayoutGroup resourcesToBuildHorizontalLayoutGroupWidget = null;

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

    public void InitializeBuildingWidget(Building newBuilding)
    {
        building = newBuilding;

        playerController = GetComponentInParent<PlayerController>();
        UIManager = playerController.GetComponentInChildren<UIManager>();

        buildButton.onClick.AddListener(StartPlacingBuilding);
        informationButton.onClick.AddListener(OpenBuildingInformationMenu);

        buildingNameText.SetText(building.buildingData.buildingName);

        resourcesToBuildNumber = building.buildingLevelsData[0].resourcesToBuild.Count();

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

            //spawnedBuildingResourceWidgets[i].Initialize(building.buildingLevelsData[0].resourcesToBuild[i].Amount, building.buildingLevelsData[0].resourcesToBuild[i].ItemData.itemIcon);
        }
    }

    private void StartPlacingBuilding()
    {
        List<bool> haveResourcesToBuild = new List<bool>();

        for (int i = 0; i < resourcesToBuildNumber; i++)
        {
            int id = building.buildingLevelsData[0].resourcesToBuild[i].ItemData.ItemId;

            if (cityManager.items[id].Amount >= building.buildingLevelsData[0].resourcesToBuild[i].Amount)
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

    private void OpenBuildingInformationMenu()
    {
        UIManager.OpenBuildingInformationMenu(building);
    }

    public void UpdateResourcesToBuild()
    {
        for (int i = 0; i < resourcesToBuildNumber; i++)
        {
            int id = building.buildingLevelsData[0].resourcesToBuild[i].ItemData.ItemId;

            spawnedBuildingResourceWidgets[i].SetResourceText(cityManager.items[id].Amount, building.buildingLevelsData[0].resourcesToBuild[i].Amount);
        }
    }
}
