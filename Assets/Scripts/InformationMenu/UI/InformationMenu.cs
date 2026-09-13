using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class InformationMenu : MonoBehaviour, IOpenable
{
    [Header("Information")]
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private TextLocalizer nameText;
    [SerializeField] private TextLocalizer descriptionText;
    [SerializeField] private Image thumbImage;
    [SerializeField] private CustomButton closeButton;

    [Header("PanelControllers")]
    [SerializeField] private InfoPanelController[] infoPanelControllers; 

    private IInformationable informationable;

    public bool IsShown { get; private set; } = false;

    public event Action OnShown;
    public event Action OnHidden;

    protected virtual void Awake()
    {
        
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
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
        slidePanel.OnHidden += HandleHidden;
    }

    protected virtual void Unsubscribe()
    {
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
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

            panelController.SetInformationable(informationable);
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
    }

    private void UpdateImage()
    {
        if (informationable == null) return;

        thumbImage.sprite = informationable.GetInformationImage();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}