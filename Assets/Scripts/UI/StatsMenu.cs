using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatsMenu : UIBehaviour
{
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI interactorsCountText;
    [SerializeField] private SlidePanel slidePanel;

    public void OpenStatsMenu(Building building)
    {
        string buildingName = building.BuildingData.BuildingName;
        buildingNameText.SetText(buildingName);

        string interactorsCountText = building.currentWorkers.Count.ToString();
        string maxInteractorsCountText = building.LevelData.maxResidentsCount.ToString();
        this.interactorsCountText.SetText(interactorsCountText + "/" + maxInteractorsCountText);

        slidePanel.OpenSlidePanel();
    }

    public void CloseStatsMenu()
    {
        slidePanel.CloseSlidePanel();
    }
}
