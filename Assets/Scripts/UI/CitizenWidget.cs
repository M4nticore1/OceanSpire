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
    public Building InteractBuilding { get; private set; }

    public int WidgetIndex { get; private set; } = 0;

    [SerializeField] private SkillsPanel skillsPanel;
    public SkillsPanel SkillsPanel => skillsPanel;

    [SerializeField] private GameObject selectedResidentMenu;
    [SerializeField] private GameObject nonSelectedResidentMenu;
    [SerializeField] private TextMeshProUGUI citizenNameText;
    [SerializeField] private LayoutGroup skillsLayoutGroup;
    [SerializeField] private CustomButton button;
    [SerializeField] private Image genderImage;
    [SerializeField] private Sprite maleIcon;
    [SerializeField] private Sprite femaleIcon;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnClicked);
    }

    public void Init(Citizen citizen)
    {
        if (citizen != null) {
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

    public void SetCitizen(Citizen citizen)
    {
        Citizen = citizen;
    }

    public void SetInteractBuilding(Building interactBuilding)
    {
        InteractBuilding = interactBuilding;
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
        if (Citizen == null) return;
        if (Citizen.NameComponent == null) return;

        citizenNameText.SetText(Citizen.NameComponent.GetName());
    }

    private void UpdateSkills()
    {
        if (Citizen == null) return;

        skillsPanel.SetSkills(Citizen.SkillsComponent);
    }

    private void UpdateGender()
    {
        if (Citizen == null) return;
        if (Citizen.GenderComponent == null) return;

        genderImage.sprite = Citizen.GenderComponent.IsMale ? maleIcon : femaleIcon;
    }

    private void OnClicked()
    {
        if (Citizen == null) {
            Debug.LogError($"[{nameof(CitizenWidget)}] Citizen is not valid!");
            return;
        }

        if (InteractBuilding == null) {
            Debug.LogError($"[{nameof(CitizenWidget)}] Interact Building is not valid!");
            return;
        }

        var interactComponent = Citizen.InteractComponent;
        var interactBuilding = Citizen.InteractComponent.InteractBuilding;

        if (interactBuilding) {
            interactComponent.RemoveInteractBuilding();
            interactComponent.TryStopInteracting(interactBuilding);
        }

        if (InteractBuilding == interactBuilding) return;
        if (InteractBuilding.CitizensHandler.Interactors.Count >= InteractBuilding.LevelDefinition.MaxHumansCount) return;

        interactComponent.SetInteractBuilding(InteractBuilding);
    }
}