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
            ShowResidentMenu();
            UpdateName();
            UpdateSkills();
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
        citizenNameText.SetText(Human.NameHandler.GetName());
    }

    private void UpdateSkills()
    {
        foreach (var skill in Human.Skills.Skills.Values) {
            SkillWidgetFactory.CreateSkillWidget(skillWidget, skillsLayoutGroup.transform, skill);
        }
    }

    private void ClickWidget()
    {
        Human.HandleClickedWorkerWidget();
        EventBus.InvokeCitizenWidgetClicked(this);
    }
}