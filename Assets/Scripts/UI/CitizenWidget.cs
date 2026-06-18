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
    public Human Human { get; private set; }
    public int WidgetIndex { get; private set; } = 0;

    [SerializeField] private SkillWidget skillWidget;
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

    public void Init(Human citizen)
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

    private void SetCitizen(Human human)
    {
        Human = human;
    }

    private void ShowResidentMenu()
    {
        selectedResidentMenu.SetActive(true);
    }

    private void HideResidentMenu()
    {
        selectedResidentMenu.SetActive(false);
    }

    private void UpdateName()
    {
        citizenNameText.SetText(Human.NameComponent.GetName());
    }

    private void UpdateSkills()
    {
        foreach (var skill in Human.SkillsComponent.Skills.Values) {
            SkillWidgetFactory.CreateSkillWidget(skillWidget, skillsLayoutGroup.transform, skill);
        }
    }

    private void UpdateGender()
    {
        genderImage.sprite = Human.GenderComponent.IsMale ? maleIcon : femaleIcon;
    }

    private void OnClicked()
    {
        var interactComponent = Human.InteractComponent;
        var interactBuilding = Human.InteractComponent.InteractBuilding;

        if (interactBuilding) {
            interactComponent.RemoveInteractBuilding();
            interactComponent.TryStopInteracting(interactBuilding);
        }
        else {
            var selectedBuilding = SelectManager.Instance.GetSelectedBuilding();
            if (!selectedBuilding) {
                Debug.LogError("SelectedBuilding is not valid", this);
            }

            if (selectedBuilding.WorkComponent.Workers.Count >= selectedBuilding.LevelData.MaxHumansCount) return;

            interactComponent.SetInteractBuilding(selectedBuilding);
        }
    }
}