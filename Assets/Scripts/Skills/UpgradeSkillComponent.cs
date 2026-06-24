using System;
using UnityEngine;

public class UpgradeSkillComponent : MonoBehaviour, IClickable
{
    [SerializeField] private SkillsComponent skillComponent;
    
    [SerializeField] private bool isClickable = false;
    public bool IsClickable => isClickable;

    public event Action OnClicked;

    private void OnEnable()
    {
        skillComponent.OnSkillXpChanged += OnSkillXpChanged;
    }

    private void OnDisable()
    {
        skillComponent.OnSkillXpChanged -= OnSkillXpChanged;
    }

    public void Click()
    {
        UpgradeLevels();
        UpdateClickable();

        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }

    public bool ShouldClick()
    {
        return isClickable;
    }

    private void UpgradeLevels()
    {
        foreach (var skill in skillComponent.Skills.Values) {
            skill.TryLevelUp();
        }
    }

    private void UpdateClickable()
    {
        foreach (var skill in skillComponent.Skills.Values) {
            if (skill.CurrentXp < 1f) continue;

            SetClickable(true);
            return;
        }

        SetClickable(false);
    }

    private void OnSkillXpChanged(SkillInstance skill, float xp)
    {
        if (xp < 1f) return;

        SetClickable(true);
    }
}