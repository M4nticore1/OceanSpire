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
        if (skill == null) {
            Debug.LogError("skill is not valid", this);
            return;
        }

        this.skill = skill;
        UpdateSkillName();
        UpdateSkillBonus();
        UpdateColor();
    }

    private void UpdateSkillName()
    {
        var item = skill.skillDefinition.LocalizeItem;
        skillName.SetLocalizationItem(item);
        skillName.UpdateText();
    }

    private void UpdateSkillBonus()
    {
        float bonus = skill.GetBonus() * 100;
        string text = $"({GetBonusText()})";
        skillBonus.SetText(text);
    }

    private void UpdateColor()
    {
        float alpha = (float)(skill.currentLevel - 1) / (SkillDefinition.maxSkillLevel / SkillsFactory.GetLevelsCount());

        var color = Color.Lerp(normalColor, highlightedColor, alpha);
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