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
        buildingName.SetLocalizationItem(building.Definition.NameLocalizationItem);

        string interactorsCountText = building.WorkComponent.CurrentWorkers.Count.ToString();
        string maxInteractorsCountText = building.LevelDefinition.MaxHumansCount.ToString();
        interactorsCount.SetText(interactorsCountText + "/" + maxInteractorsCountText);

        slidePanel.Open();
    }

    public void CloseStatsMenu()
    {
        slidePanel.Close();
    }
}
