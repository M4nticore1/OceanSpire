using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ResidentWidgetState
{
    UnemployedResident,
    Worker,
    SelectedBuildingWorker,
    NonSelectedWorker,
}

public class ResidentWidget : MonoBehaviour
{
    [HideInInspector] public Creature resident = null;
    [HideInInspector] public Building selectedBuilding = null;

    public int widgetIndex = 0;
    private ResidentWidgetState residentWidgetState = ResidentWidgetState.NonSelectedWorker;

    [SerializeField] private GameObject selectedResidentMenu = null;
    [SerializeField] private GameObject nonSelectedResidentMenu = null;
    [SerializeField] private TextMeshProUGUI residentNameText = null;
    [SerializeField] private Button residentWidgetButton = null;

    private void OnEnable()
    {
        residentWidgetButton.onClick.AddListener(ClickWidget);
    }

    private void OnDisable()
    {
        residentWidgetButton.onClick.RemoveAllListeners();
    }

    public void InitializeResidentWidget(Creature resident, Building selectedBuilding)
    {
        this.resident = resident;
        this.selectedBuilding = selectedBuilding;

        if (resident) {
            ShowResidentMenu();
        }
        else  {
            HideResidentMenu();
        }
    }

    public void SetResident(Resident resident)
    {
        this.resident = resident;

        ShowResidentMenu();
    }

    public void ShowResidentMenu()
    {
        selectedResidentMenu.SetActive(true);
        residentNameText.SetText(resident.firstName + "\n" + resident.lastName);
    }

    public void HideResidentMenu()
    {
        selectedResidentMenu.SetActive(false);
    }

    private void ClickWidget()
    {
        if (resident.workBuilding) {
            if (resident.workBuilding == selectedBuilding) {
                resident.RemoveWork();
                resident.DecideAction();
            }
            else {
                if (selectedBuilding.workers.Count < selectedBuilding.ConstructionLevelsData[selectedBuilding.LevelIndex].maxResidentsCount) {
                    resident.SetWork(selectedBuilding);
                    resident.DecideAction();
                }
            }
        }
        else {
            if (selectedBuilding.workers.Count < selectedBuilding.ConstructionLevelsData[selectedBuilding.LevelIndex].maxResidentsCount) {
                resident.SetWork(selectedBuilding);
                resident.DecideAction();
            }
        }
        EventBus.Instance.InvokeResidentWidgetClicked(this);
    }
}
