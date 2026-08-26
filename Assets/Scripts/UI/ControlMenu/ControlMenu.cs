using System;
using TMPro;
using UnityEngine;

public abstract class ControlMenu : MonoBehaviour, IOpenable
{
    [Header("Main")]
    [SerializeField] private GameObject content;
    [SerializeField] private CustomButton closeButton;

    [Header("Target Info")]
    [SerializeField] private TextLocalizer targetNameText;
    [SerializeField] private TextLocalizer targetDescriptionText;

    public bool IsShowed { get; private set; } = false;

    public event Action OnShowed;
    public event Action OnHidden;

    protected virtual void Awake()
    {

    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    protected virtual void Subscribe()
    {
        closeButton.OnReleased.AddListener(HandleCloseButtonClicked);
    }

    protected virtual void Unsubscribe()
    {
        closeButton.OnReleased.RemoveListener(HandleCloseButtonClicked);
    }

    protected virtual bool ShouldSubscribe()
    {
        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        return true;
    }

    public void Show()
    {
        IsShowed = true;
        content.SetActive(true);

        InputStateManager.Instance.AddBlockTarget(this);
        OnShow();

        OnShowed?.Invoke();
    }

    public void Hide()
    {
        IsShowed = false;
        content.SetActive(false);

        InputStateManager.Instance.RemoveBlockTarget(this);
        OnHide();

        OnHidden?.Invoke();
    }

    protected virtual void OnShow()
    {
        UpdateMenu();
        UpdateTargetNameText();
        UpdateTargetDescriptionText();
    }

    protected virtual void OnHide()
    {

    }

    protected abstract void UpdateMenu();

    protected abstract ILocalizable GetTargetNameText();

    protected abstract ILocalizable GetTargetDescriptionText();

    private void UpdateTargetNameText()
    {
        if (targetNameText == null) return;

        targetNameText.SetPlaceHolderLocalization(GetTargetNameText());
    }

    private void UpdateTargetDescriptionText()
    {
        if (targetDescriptionText == null) return;

        targetDescriptionText.SetPlaceHolderLocalization(GetTargetDescriptionText());
    }

    private void HandleCloseButtonClicked()
    {
        Hide();
    }
}