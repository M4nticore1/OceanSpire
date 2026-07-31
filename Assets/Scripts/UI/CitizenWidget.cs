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
    public Citizen Citizen { get; private set; }
    public int WidgetIndex { get; private set; } = 0;

    [SerializeField] private SkillsPanel skillsPanel;
    public SkillsPanel SkillsPanel => skillsPanel;

    [SerializeField] private GameObject selectedResidentMenu;
    [SerializeField] private GameObject nonSelectedResidentMenu;
    [SerializeField] private TextMeshProUGUI citizenNameText;
    [SerializeField] private LayoutGroup skillsLayoutGroup;
    [SerializeField] private Button button;
    [SerializeField] private Image genderImage;
    [SerializeField] private Sprite maleIcon;
    [SerializeField] private Sprite femaleIcon;

    private void OnEnable()
    {
        button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClicked);
    }

    public void Init(Citizen citizen)
    {
        if (citizen) {
            SetCitizen(citizen);
            ShowResidentMenu();
            UpdateName();
            UpdateSkills();
            UpdateGender();
        }
        else {
            HideResidentMenu();
        }
    }

    private void SetCitizen(Citizen citizen)
    {
        Citizen = citizen;
    }

    private void ShowResidentMenu()
    {
        selectedResidentMenu.SetActive(true);
        nonSelectedResidentMenu.SetActive(false);
    }

    private void HideResidentMenu()
    {
        selectedResidentMenu.SetActive(false);
        nonSelectedResidentMenu.SetActive(true);
    }

    private void UpdateName()
    {
        citizenNameText.SetText(Citizen.NameComponent.GetName());
    }

    private void UpdateSkills()
    {
        skillsPanel.SetSkills(Citizen.SkillsComponent);
    }

    private void UpdateGender()
    {
        genderImage.sprite = Citizen.GenderComponent.IsMale ? maleIcon : femaleIcon;
    }

    private void OnClicked()
    {
        if (!Citizen) return;

        var interactComponent = Citizen.InteractComponent;
        var interactBuilding = Citizen.InteractComponent.InteractBuilding;

        if (interactBuilding) {
            interactComponent.RemoveInteractBuilding();
            interactComponent.TryStopInteracting(interactBuilding);
        }

        var selectedBuilding = SelectManager.Instance.GetSelectedBuilding();

        if (!selectedBuilding) return;
        if (selectedBuilding == interactBuilding) return;
        if (selectedBuilding.WorkComponent.Workers.Count >= selectedBuilding.LevelDefinition.MaxHumansCount) return;

        interactComponent.SetInteractBuilding(selectedBuilding);
    }
}