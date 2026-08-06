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

        string interactorsCountText = building.CitizensHandler.CurrentInteractors.Count.ToString();
        string maxInteractorsCountText = building.LevelDefinition.MaxHumansCount.ToString();
        interactorsCount.SetText(interactorsCountText + "/" + maxInteractorsCountText);

        slidePanel.Show();
    }

    public void CloseStatsMenu()
    {
        slidePanel.Hide();
    }
}
