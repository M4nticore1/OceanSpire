using System;
using System.Collections;
using UnityEngine;

public class UpgradeSkillsController : MonoBehaviour, IClickable
{
    [SerializeField] private SkillsComponent skillComponent;
    [SerializeField] private SelectComponent selectComponent;
    
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

    private void Start()
    {
        UpdateClickable();
    }

    public void Click()
    {
        UpgradeLevels();
        UpdateClickable();
        selectComponent.Deselect();

        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }

    public bool ShouldClick()
    {
        if (!isClickable) return false;

        return true;
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
        if (!skill.ShouldLevelUp()) return;

        SetClickable(true);
    }
}