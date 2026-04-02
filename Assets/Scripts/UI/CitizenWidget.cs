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
    public Human human { get; private set; }
    public int widgetIndex = 0;

    [SerializeField] private GameObject selectedResidentMenu;
    [SerializeField] private GameObject nonSelectedResidentMenu;
    [SerializeField] private TextMeshProUGUI citizenNameText;
    [SerializeField] private Button button;

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
        this.human = citizen;
        ShowResidentMenu();
    }

    public void RemoveCitizen()
    {
        HideResidentMenu();
    }

    public void ShowResidentMenu()
    {
        selectedResidentMenu.SetActive(true);
        citizenNameText.SetText(human.firstName + "\n" + human.lastName);
    }

    public void HideResidentMenu()
    {
        selectedResidentMenu.SetActive(false);
    }

    private void ClickWidget()
    {
        human.HandleClickedWorkerWidget();
        EventBus.InvokeCitizenWidgetClicked(this);
    }
}
