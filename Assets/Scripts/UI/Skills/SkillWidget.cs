using TMPro;
using UnityEngine;

public class SkillWidget : MonoBehaviour
{
    private SkillInstance skill;

    [SerializeField] private TextLocalizer skillName;
    [SerializeField] private TextMeshProUGUI skillBonus;

    public void Init(SkillInstance skill)
    {
        this.skill = skill;
        AssignSkillName();
        AssignSkillBonus();
    }

    private void AssignSkillName()
    {
        LocalizationItem item = skill.skillDefinition.LocalizeItem;
        skillName.SetLocalizationItem(item);
        skillName.UpdateText();
    }

    private void AssignSkillBonus()
    {
        float bonus = (skill.GetBonus() * 100);
        string text = $"(+{bonus})";
        skillBonus.SetText(text);
    }
}