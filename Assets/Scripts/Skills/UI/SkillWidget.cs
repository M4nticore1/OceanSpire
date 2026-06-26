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
        UpdateBonusTextAndColor();
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

    private void UpdateBonusTextAndColor()
    {
        if (Skill == null) return;

        float bonusValue = Skill.GetBonus() * 100;
        string bonusText = $"+{bonusValue}%";

        float alpha = (float)(Skill.CurrentLevel - 1) / (SkillDefinition.MaxSkillLevel / SkillsFactory.GetLevelsCount());
        var color = Color.Lerp(normalColor, highlightedColor, alpha);
        string hex = ColorUtility.ToHtmlStringRGB(color);

        string finalString = $"<color=#{hex}>({bonusText})</color>";

        skillBonus.SetText(finalString);
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
        UpdateBonusTextAndColor();
    }
}