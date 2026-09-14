using System;
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

    private IInformationable informationable;

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

        SetPanelTargets();
        UpdateNameText();
        UpdateDescriptionText();
        UpdateImage();
        UpdateScrollRect();

        InputStateManager.Instance.AddInputBlockTarget(this);

        OnShown?.Invoke();
    }

    public void Show(IInformationable informationable)
    {
        if (informationable == null) {
            Debug.LogError($"[{nameof(InformationMenu)}] Informationable is not valid!");
            return;
        }

        this.informationable = informationable;
        Show();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    private void HandleHidden()
    {
        IsShown = false;
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
    }

    private void SetPanelTargets()
    {
        if (informationable == null) return;

        foreach (var panelController in infoPanelControllers) {
            if (panelController == null) {
                Debug.LogError($"[{nameof(InformationMenu)}] Panel Controller is not valid!");
                continue;
            }

            panelController.SetInformationableAndUpdate(informationable);
        }
    }

    private void UpdateNameText()
    {
        if (informationable == null) return;

        nameText.SetLocalizationItem(informationable.GetInformationName());
    }

    private void UpdateDescriptionText()
    {
        if (informationable == null) return;

        descriptionText.SetLocalizationItem(informationable.GetInformationDescription());
        descriptionTextFitSize.UpdateSize();
    }

    private void UpdateImage()
    {
        if (informationable == null) return;

        thumbImage.sprite = informationable.GetInformationImage();
    }

    private void UpdateScrollRect()
    {
        scrollRect.verticalNormalizedPosition = 1f;
        scrollRectFitSize.UpdateSize();
    }

    private void HandleCloseButtonClicked()
    {
        Hide();
    }
}