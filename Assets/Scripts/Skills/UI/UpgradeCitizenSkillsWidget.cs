using UnityEngine;

public class UpgradeCitizenSkillsWidget : MonoBehaviour
{
    [SerializeField] private SkillsComponent skillComponent;
    [SerializeField] private GameObject content;

    private bool isShown => content.activeSelf;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        skillComponent.OnSkillXpChanged += OnSkillXpChanged;
        skillComponent.OnSkillLevelChanged += OnSkillLevelChanged;
    }

    private void OnDisable()
    {
        skillComponent.OnSkillXpChanged -= OnSkillXpChanged;
        skillComponent.OnSkillLevelChanged -= OnSkillLevelChanged;
    }

    private void Start()
    {
        UpdateDisplayed();
    }

    private void UpdateDisplayed()
    {
        if (ShouldDisplay()) {
            Show();
        }
        else {
            Hide();
        }
    }

    private void Show()
    {
        if (isShown) return;

        content.SetActive(true);
    }

    private void Hide()
    {
        if (!isShown) return;

        content.SetActive(false);
    }
    
    private void OnSkillXpChanged(SkillInstance skill, float xp)
    {
        if (!skill.ShouldLevelUp()) return;

        UpdateDisplayed();
    }

    private void OnSkillLevelChanged(SkillInstance skill, int level)
    {
        UpdateDisplayed();
    }

    private bool ShouldDisplay()
    {
        foreach (var skill in skillComponent.Skills.Values) {
            if (!skill.ShouldLevelUp()) continue;

            return true;
        }

        return false;
    }
}