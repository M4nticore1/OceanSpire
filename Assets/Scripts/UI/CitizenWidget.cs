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

public class CitizenWidget : MonoBehaviour
{
    [HideInInspector] public Human citizen = null;

    public int widgetIndex = 0;

    [SerializeField] private GameObject selectedResidentMenu = null;
    [SerializeField] private GameObject nonSelectedResidentMenu = null;
    [SerializeField] private TextMeshProUGUI citizenNameText = null;
    [SerializeField] private Button button = null;

    private void OnEnable()
    {
        button.onClick.AddListener(ClickWidget);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(ClickWidget);
    }

    public void Init(Human citizen)
    {
        if (citizen) {
            SetCitizen(citizen);
        }
        else {
            HideResidentMenu();
        }
    }

    public void SetCitizen(Human citizen)
    {
        this.citizen = citizen;
        ShowResidentMenu();
    }

    public void RemoveCitizen()
    {
        HideResidentMenu();
    }

    public void ShowResidentMenu()
    {
        selectedResidentMenu.SetActive(true);
        citizenNameText.SetText(citizen.firstName + "\n" + citizen.lastName);
    }

    public void HideResidentMenu()
    {
        selectedResidentMenu.SetActive(false);
    }

    private void ClickWidget()
    {
        EventBus.InvokeCitizenWidgetClicked(this);
    }
}
