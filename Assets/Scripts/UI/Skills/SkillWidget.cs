using TMPro;
using UnityEngine;

public class SkillWidget : MonoBehaviour
{
    private SkillInstance skill;

    [SerializeField] private TextLocalizer skillName;
    [SerializeField] private TextMeshProUGUI skillBonus;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightedColor;

    public void Init(SkillInstance skill)
    {
        this.skill = skill;
        AssignSkillName();
        AssignSkillBonus();
        AssignHighlight();
    }

    private void AssignSkillName()
    {
        LocalizationItem item = skill.skillDefinition.LocalizeItem;
        skillName.SetLocalizationItem(item);
        skillName.UpdateText();
    }

    private void AssignSkillBonus()
    {
        float bonus = skill.GetBonus() * 100;
        string text = $"({GetBonusText()})";
        skillBonus.SetText(text);
    }

    private void AssignHighlight()
    {
        float alpha = (float)(skill.currentLevel - 1) / (SkillDefinition.maxSkillLevel / SkillsFactory.GetLevelsCount());

        Color color = Color.Lerp(normalColor, highlightedColor, alpha);
        string hex = ColorUtility.ToHtmlStringRGBA(color);

        string bonus = GetBonusText();
        string highlighted = $"<color=#{hex}>{bonus}</color>";

        string newText = skillBonus.text.Replace(bonus, highlighted);
        skillBonus.SetText(newText);
    }

    private string GetBonusText()
    {
        string bonus = (skill.GetBonus() * 100).ToString();
        string text = $"+{bonus}%";

        return text;
    }
}