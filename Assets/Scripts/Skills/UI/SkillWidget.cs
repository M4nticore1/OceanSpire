using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillWidget : MonoBehaviour
{
    public SkillInstance Skill { get; private set; }

    [Header("Main")]
    [SerializeField] private CustomButton button;
    [SerializeField] private TextLocalizer skillName;

    [Header("Progress")]
    [SerializeField] private Image skillProgress;

    [Header("Highlight")]
    [SerializeField] private Image skillHighlight;
    [SerializeField] private float highlightPower = 0.25f;

    [Header("Description")]
    [SerializeField] private RectTransform descriptionTransform;
    public RectTransform DescriptionTransform => descriptionTransform;

    [Header("Bonus")]
    [SerializeField] private TextMeshProUGUI skillBonus;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightedColor;

    [Header("Background")]
    [SerializeField] private Image background;
    [SerializeField] private Color evenBackgroundColor = Color.white;
    [SerializeField] private Color oddBackgroundColor = Color.white;

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
        UpdateBackgroundColor();

        skill.OnXpChanged += OnSkillXpChanged;
        skill.OnLevelChanged += OnSkillLevelChanged;
    }

    public void SetHighlighted(bool highlighted)
    {
        var color = skillHighlight.color;
        skillHighlight.color = new Color(color.r, color.g, color.b, highlighted ? highlightPower : 0);
    }

    private void UpdateSkillName()
    {
        if (Skill == null) return;

        var item = Skill.SkillDefinition.SkillNameLocalization;
        skillName.SetLocalizationItem(item);
    }

    private void UpdateBonusTextAndColor()
    {
        if (Skill == null) return;

        float bonusValue = Skill.GetBonus() * 100;
        string bonusText = $"+{bonusValue}%";

        int stagesCount = SkillsData.GetLevelsCountByGameStage();
        float alpha = 0f;

        if (stagesCount > 0 && SkillDefinition.MaxSkillLevel > 0) {
            float divider = (float)SkillDefinition.MaxSkillLevel / stagesCount;
            if (divider != 0) {
                alpha = (Skill.CurrentLevel - 1) / divider;
            }
        }

        alpha = Mathf.Clamp01(alpha);

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

    private void UpdateBackgroundColor()
    {
        background.color = (int)Skill.SkillDefinition.SkillId % 2 == 0 ? evenBackgroundColor : oddBackgroundColor;
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