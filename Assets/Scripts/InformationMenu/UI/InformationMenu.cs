using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class InformationMenu : MonoBehaviour, IOpenable
{
    [Header("Information")]
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TextLocalizer nameText;
    [SerializeField] private TextLocalizer descriptionText;
    [SerializeField] private Image thumbImage;
    [SerializeField] private CustomButton closeButton;

    private IInformationable informationable;

    public bool IsShowed { get; private set; } = false;

    public event Action OnShowed;
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
        IsShowed = true;
        slidePanel.Show();

        UpdateNameText();
        UpdateDescriptionText();
        UpdateImage();
        InputStateManager.Instance.AddBlockTarget(this);

        OnShowed?.Invoke();
    }

    public void Show(IInformationable informationable)
    {
        if (informationable == null) {
            Debug.LogError($"[{nameof(InformationMenu)}] Informationable is not valid!");
            return;
        }

        if (!informationable.GetInformationName()) {
            Debug.LogError($"[{nameof(InformationMenu)}] Information Name is not valid!");
            return;
        }

        if (!informationable.GetInformationDescription()) {
            Debug.LogError($"[{nameof(InformationMenu)}] Information Description is not valid!");
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
        IsShowed = false;
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
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