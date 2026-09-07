using UnityEngine;
using UnityEngine.UI;

public class UpgradeCitizenSkillsWidget : MonoBehaviour
{
    [SerializeField] private SkillsComponent skillComponent;
    [SerializeField] private GameObject content;
    [SerializeField] private LayoutElement layoutElement;

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
        UpdateIgnoreLayout();
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
        UpdateIgnoreLayout();
    }

    private void Hide()
    {
        if (!isShown) return;

        content.SetActive(false);
        UpdateIgnoreLayout();
    }

    private void UpdateIgnoreLayout()
    {
        if (layoutElement != null) {
            layoutElement.ignoreLayout = !isShown;
        }
    }
    
    private void OnSkillXpChanged(SkillsComponent skillsComponent, SkillInstance skillInstance)
    {
        if (skillInstance.ShouldLevelUp()) {
            UpdateDisplayed();
        }
    }

    private void OnSkillLevelChanged(SkillsComponent skillsComponent, SkillInstance skillInstance)
    {
        UpdateDisplayed();
    }

    private bool ShouldDisplay()
    {
        foreach (var skill in skillComponent.SkillsDict.Values) {
            if (skill.ShouldLevelUp())
                return true;
        }

        return false;
    }
}