using System;
using TMPro;
using UnityEngine;

public class SkillWidget : MonoBehaviour
{
    public SkillInstance Skill { get; private set; }

    [SerializeField] private CustomButton button;
    [SerializeField] private TextLocalizer skillName;
    [SerializeField] private TextMeshProUGUI skillBonus;

    [SerializeField] private RectTransform descriptionTransform;
    public RectTransform DescriptionTransform => descriptionTransform;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightedColor;

    public static event Action<SkillWidget> OnSkillWidgetSelected;
    public static event Action<SkillWidget> OnSkillWidgetDeselected;

    private void OnEnable()
    {
        button.OnSelected.AddListener(OnButtonSelected);
        button.OnDeselected.AddListener(OnButtonDeselected);
    }

    private void OnDisable()
    {
        button.OnSelected.RemoveListener(OnButtonSelected);
        button.OnDeselected.RemoveListener(OnButtonDeselected);
    }

    public void Init(SkillInstance skill)
    {
        if (skill == null) {
            Debug.LogError("skill is not valid", this);
            return;
        }

        this.Skill = skill;
        UpdateSkillName();
        UpdateSkillBonus();
        UpdateColor();
    }

    private void UpdateSkillName()
    {
        var item = Skill.SkillDefinition.SkillNameLocalization;
        skillName.SetLocalizationItem(item);
        skillName.UpdateText();
    }

    private void UpdateSkillBonus()
    {
        float bonus = Skill.GetBonus() * 100;
        string text = $"({GetBonusText()})";
        skillBonus.SetText(text);
    }

    private void UpdateColor()
    {
        float alpha = (float)(Skill.currentLevel - 1) / (SkillDefinition.maxSkillLevel / SkillsFactory.GetLevelsCount());

        var color = Color.Lerp(normalColor, highlightedColor, alpha);
        string hex = ColorUtility.ToHtmlStringRGBA(color);

        string bonus = GetBonusText();
        string highlighted = $"<color=#{hex}>{bonus}</color>";

        string newText = skillBonus.text.Replace(bonus, highlighted);
        skillBonus.SetText(newText);
    }

    private void OnButtonSelected()
    {
        OnSkillWidgetSelected?.Invoke(this);
    }

    private void OnButtonDeselected()
    {
        OnSkillWidgetDeselected?.Invoke(this);
    }

    private string GetBonusText()
    {
        string bonus = (Skill.GetBonus() * 100).ToString();
        string text = $"+{bonus}%";

        return text;
    }
}