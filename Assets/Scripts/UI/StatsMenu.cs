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
        buildingName.SetLocalizationItem(building.BuildingData.NameLocalizationItem);
        buildingName.UpdateText();

        string interactorsCountText = building.WorkComponent.CurrentWorkers.Count.ToString();
        string maxInteractorsCountText = building.LevelData.MaxHumansCount.ToString();
        interactorsCount.SetText(interactorsCountText + "/" + maxInteractorsCountText);

        slidePanel.Show();
    }

    public void CloseStatsMenu()
    {
        slidePanel.Hide();
    }
}
