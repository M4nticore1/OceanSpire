using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InformationMenu : MonoBehaviour, IOpenable
{
    public static InformationMenu Instance { get; private set; }

    [Header("Information")]
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private TextLocalizer nameText;
    [SerializeField] private Image thumbImage;
    [SerializeField] private CustomButton closeButton;

    [Header("Description Text")]
    [SerializeField] private TextLocalizer descriptionText;
    [SerializeField] private FitSizeToContent descriptionTextFitSize;

    [Header("Scroll Rect")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private FitSizeToContent scrollRectFitSize;

    [Header("PanelControllers")]
    [SerializeField] private InfoPanelController[] infoPanelControllers; 

    private List<IInformationable> informationables = new();
    private IInformationable shownInformationable => informationables.Count > 0 ? informationables[informationables.Count - 1] : null;

    public bool IsShown { get; private set; } = false;

    public event Action OnShown;
    public event Action OnHidden;

    protected virtual void Awake()
    {
        if (Instance != null) {
            return;
        }

        Instance = this;
    }

    protected virtual void OnEnable()
    {
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
    }

    protected virtual void Subscribe()
    {
        closeButton.OnReleased.AddListener(HandleCloseButtonClicked);
        slidePanel.OnHidden += HandleHidden;
    }

    protected virtual void Unsubscribe()
    {
        closeButton.OnReleased.RemoveListener(HandleCloseButtonClicked);
        slidePanel.OnHidden -= HandleHidden;
    }

    public void Show()
    {
        IsShown = true;
        slidePanel.Show();

        UpdateDisplayed();

        if (!InputStateManager.Instance.InputBlockTargets.Contains(this)) {
            InputStateManager.Instance.AddInputBlockTarget(this);
        }

        OnShown?.Invoke();
    }

    public void Show(IInformationable informationable)
    {
        if (informationable == null) {
            Debug.LogError($"[{nameof(InformationMenu)}] Informationable is not valid!");
            return;
        }

        informationables.Add(informationable);

        Show();
    }

    public void Hide()
    {
        if (informationables.Count > 0) {
            informationables.RemoveAt(informationables.Count - 1);
        }

        if (informationables.Count <= 0) {
            slidePanel.Hide();
        }
        else {
            UpdateDisplayed();
        }
    }

    private void HandleHidden()
    {
        IsShown = false;
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
    }

    private void UpdateDisplayed()
    {
        SetPanelTargets();
        UpdateNameText();
        UpdateDescriptionText();
        UpdateImage();
        UpdateScrollRect();
    }

    private void SetPanelTargets()
    {
        if (shownInformationable == null) return;

        foreach (var panelController in infoPanelControllers) {
            if (panelController == null) {
                Debug.LogError($"[{nameof(InformationMenu)}] Panel Controller is not valid!");
                continue;
            }

            panelController.SetInformationableAndUpdate(shownInformationable);
        }
    }

    private void UpdateNameText()
    {
        if (shownInformationable == null) return;

        nameText.SetLocalizationItem(shownInformationable.GetInformationName());
    }

    private void UpdateDescriptionText()
    {
        if (shownInformationable == null) return;

        descriptionText.SetLocalizationItem(shownInformationable.GetInformationDescription());
        descriptionTextFitSize.UpdateSize();
    }

    private void UpdateImage()
    {
        if (shownInformationable == null) return;

        thumbImage.sprite = shownInformationable.GetInformationIcon();
    }

    private void UpdateScrollRect()
    {
        scrollRect.verticalNormalizedPosition = 1f;
        scrollRectFitSize.RunUpdateSizeEndOfFrame();
    }

    private void HandleCloseButtonClicked()
    {
        Hide();
    }
}