using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatsMenu : UIBehaviour
{
    [SerializeField] private TextLocalizer buildingName;
    [SerializeField] private TextMeshProUGUI interactorsCount;
    [SerializeField] private SlidePanel slidePanel;

    public void OpenStatsMenu(Building building)
    {
        buildingName.SetLocalizationItem(building.BuildingData.LocalizationItem);
        buildingName.UpdateText();

        string interactorsCountText = building.WorkComponent.EnteredWorkers.Count.ToString();
        string maxInteractorsCountText = building.LevelData.maxResidentsCount.ToString();
        interactorsCount.SetText(interactorsCountText + "/" + maxInteractorsCountText);

        slidePanel.Open();
    }

    public void CloseStatsMenu()
    {
        slidePanel.Close();
    }
}
