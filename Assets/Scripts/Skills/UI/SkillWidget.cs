using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillWidget : MonoBehaviour
{
    public SkillInstance Skill { get; private set; }

    [SerializeField] private CustomButton button;
    [SerializeField] private TextLocalizer skillName;
    [SerializeField] private TextMeshProUGUI skillBonus;
    [SerializeField] private Image skillProgress;

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

    private void OnDestroy()
    {
        if (Skill == null) return;

        Skill.OnXpChanged -= OnSkillXpChanged;
        Skill.OnLevelChanged -= OnSkillLevelChanged;
    }

    public void Init(SkillInstance skill)
    {
        if (skill == null) {
            Debug.LogError("skill is not valid", this);
            return;
        }

        Skill = skill;
        UpdateSkillName();
        UpdateBonusText();
        UpdateBonusColor();
        UpdateProgress();

        skill.OnXpChanged += OnSkillXpChanged;
        skill.OnLevelChanged += OnSkillLevelChanged;
    }

    private void UpdateSkillName()
    {
        if (Skill == null) return;

        var item = Skill.SkillDefinition.SkillNameLocalization;
        skillName.SetLocalizationItem(item);
        skillName.UpdateText();
    }

    private void UpdateBonusText()
    {
        if (Skill == null) return;

        float bonus = Skill.GetBonus() * 100;
        string text = $"({GetBonusText()})";
        skillBonus.SetText(text);
    }

    private void UpdateBonusColor()
    {
        if (Skill == null) return;

        float alpha = (float)(Skill.CurrentLevel - 1) / (SkillDefinition.maxSkillLevel / SkillsFactory.GetLevelsCount());

        var color = Color.Lerp(normalColor, highlightedColor, alpha);
        string hex = ColorUtility.ToHtmlStringRGBA(color);

        string bonus = GetBonusText();
        string highlighted = $"<color=#{hex}>{bonus}</color>";

        string newText = skillBonus.text.Replace(bonus, highlighted);
        skillBonus.SetText(newText);
    }

    private void UpdateProgress()
    {
        if (Skill == null) return;

        var fillAmount = Skill.CurrentXp;
        skillProgress.fillAmount = fillAmount;
    }

    private void OnButtonSelected()
    {
        OnSkillWidgetSelected?.Invoke(this);
    }

    private void OnButtonDeselected()
    {
        OnSkillWidgetDeselected?.Invoke(this);
    }

    private void OnSkillXpChanged(SkillInstance skill, float xp)
    {
        UpdateProgress();
    }

    private void OnSkillLevelChanged(SkillInstance skill, int level)
    {
        UpdateBonusText();
        UpdateBonusColor();
    }

    private string GetBonusText()
    {
        if (Skill == null) return string.Empty;

        string bonus = (Skill.GetBonus() * 100).ToString();
        string text = $"+{bonus}%";

        return text;
    }
}