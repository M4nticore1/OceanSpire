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

    public int widgetIndex = 0;

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
        residentWidgetButton.onClick.RemoveListener(ClickWidget);
    }

    public void InitializeResidentWidget(Creature resident)
    {
        this.resident = resident;
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
        EventBus.InvokeResidentWidgetClicked(this);
    }
}
