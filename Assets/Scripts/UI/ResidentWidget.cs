using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
    [HideInInspector] public Resident resident = null;
    [HideInInspector] public Building selectedBuilding = null;
    private UIManager uiManager = null;

    public int widgetIndex = 0;
    private ResidentWidgetState residentWidgetState = ResidentWidgetState.NonSelectedWorker;

    [SerializeField] private GameObject selectedResidentMenu = null;
    [SerializeField] private GameObject nonSelectedResidentMenu = null;
    [SerializeField] private TextMeshProUGUI residentNameText = null;
    [SerializeField] private Button residentWidgetButton = null;

    //public static event Action OnWorkerAdd;
    //public static event Action OnWorkerRemove;

    private void OnEnable()
    {
        residentWidgetButton.onClick.AddListener(ClickWidget);
    }

    private void OnDisable()
    {
        residentWidgetButton.onClick.RemoveAllListeners();
    }

    public void InitializeResidentWidget(Resident resident, Building selectedBuilding, UIManager uiManager)
    {
        this.resident = resident;
        this.selectedBuilding = selectedBuilding;
        this.uiManager = uiManager;
        //this.widgetIndex = widgetIndex;

        if (resident)
        {
            ShowResidentMenu();

            //if (resident.isWorker && resident.workBuilding == selectedBuilding)
            //{
            //    residentWidgetState = ResidentWidgetState.SelectedBuildingWorker;
            //    ShowResidentMenu();
            //}
            //else if(resident.isWorker && resident.workBuilding != selectedBuilding)
            //{
            //    residentWidgetState = ResidentWidgetState.Worker;
            //    ShowResidentMenu();
            //}
            //else
            //{
            //    residentWidgetState = ResidentWidgetState.UnemployedResident;
            //    ShowResidentMenu();
            //}
        }
        else
        {
            //residentWidgetState = ResidentWidgetState.NonSelectedWorker;
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
        //nonSelectedResidentMenu.SetActive(false);

        residentNameText.SetText(resident.firstName + "\n" + resident.lastName);
    }

    public void HideResidentMenu()
    {
        selectedResidentMenu.SetActive(false);
        //nonSelectedResidentMenu.SetActive(true);
    }

    private void ClickWidget()
    {
        if (resident.isWorker)
        {
            if (resident.workBuilding == selectedBuilding)
            {
                resident.RemoveWorkBuilding();
            }
            else
            {
                if (selectedBuilding.workersCount < selectedBuilding.buildingLevelsData[selectedBuilding.levelIndex].maxResidentsCount)
                {
                    resident.RemoveWorkBuilding();
                    resident.SetWorkBuilding(selectedBuilding);
                }
            }
        }
        else
        {
            if (selectedBuilding.workersCount < selectedBuilding.buildingLevelsData[selectedBuilding.levelIndex].maxResidentsCount)
            {
                resident.SetWorkBuilding(selectedBuilding);
            }
        }

        uiManager.SelectBuildingWorker(this);
    }
}
