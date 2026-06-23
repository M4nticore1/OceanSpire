using UnityEngine;

public class SkillDescriptionWidget : MonoBehaviour
{
    [SerializeField] private TextLocalizer skillNameText;
    [SerializeField] private TextLocalizer skillDescriptionText;

    public SkillInstance Skill { get; private set; }

    public void Init(SkillInstance skillInstance)
    {
        if (skillInstance == null) {
            Debug.LogError("skillInstance is not valid");
            return;
        }

        Skill = skillInstance;

        UpdateNameText();
        UpdateDescriptionText();
    }

    private void UpdateNameText()
    {
        skillNameText.SetLocalizationItem(Skill.SkillDefinition.SkillNameLocalization);
        skillNameText.UpdateText();
    }

    private void UpdateDescriptionText()
    {
        skillDescriptionText.SetLocalizationItem(Skill.SkillDefinition.SkillDescriptionLocalization);
        skillDescriptionText.SetPlaceHolderLocalization(Skill);
        skillDescriptionText.UpdateText();
    }
}