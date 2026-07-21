using System;
using UnityEngine;
using UnityEngine.UI;

public class InformationMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TextLocalizer nameText;
    [SerializeField] private TextLocalizer descriptionText;
    [SerializeField] private Image thumbImage;
    [SerializeField] private CustomButton closeButton;

    private Building building;

    public event Action OnShown;
    public event Action OnHidden;

    private void OnEnable()
    {
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
    }

    public void Show()
    {
        OnShown?.Invoke();
    }

    public void Show(Building building)
    {
        if (!building) {
            Debug.LogError($"[{nameof(InformationMenu)}] Building is not valid!");
            return;
        }

        if (!building.Definition) return;
        if (!building.Definition.NameLocalizationItem) return;
        if (!building.Definition.DescriptionLocalizationItem) return;

        this.building = building;
        slidePanel.Show();

        UpdateNameText();
        UpdateDescriptionText();
        UpdateImage();

        InputStateManager.Instance.SetGameplayInputBlocked(true);

        Show();
    }

    public void Hide()
    {
        slidePanel.Hide();

        InputStateManager.Instance.SetGameplayInputBlocked(false);

        OnHidden?.Invoke();
    }

    private void UpdateNameText()
    {
        if (!building) return;

        var definition = building.Definition;
        if (!definition) return;

        nameText.SetLocalizationItem(definition.NameLocalizationItem);
    }

    private void UpdateDescriptionText()
    {
        if (!building) return;

        var definition = building.Definition;
        if (!definition) return;

        descriptionText.SetLocalizationItem(definition.DescriptionLocalizationItem);
    }

    private void UpdateImage()
    {
        if (!building) return;

        var definition = building.LevelDefinition;
        if (!definition) return;

        thumbImage.sprite = definition.BuildingThumb;
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}