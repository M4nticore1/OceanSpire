using UnityEngine;

public class UpgradeCitizenSkillsWidget : MonoBehaviour
{
    [SerializeField] private SkillsComponent skillComponent;
    [SerializeField] private GameObject content;

    private bool isDisplayed;

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

    private void UpdateDisplayed()
    {
        if (ShouldDisplay()) {
            Display();
        }
        else {
            Hide();
        }
    }

    private void Display()
    {
        if (isDisplayed) return;

        content.SetActive(true);
        isDisplayed = true;
    }

    private void Hide()
    {
        if (!isDisplayed) return;

        content.SetActive(false);
        isDisplayed = false;
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