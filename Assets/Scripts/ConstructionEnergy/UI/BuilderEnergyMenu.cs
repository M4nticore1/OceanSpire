using System;
using UnityEngine;

public class BuilderEnergyMenu : MonoBehaviour, IOpenable
{
    [Header("Main")]
    [SerializeField] private BuilderEnergyManager constructionEnergyManager;

    [Header("UI")]
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private TextLocalizer currentEnergyText;
    [SerializeField] private TextLocalizer nextChargeText;

    private bool isShowed;

    public event Action OnShowed;
    public event Action OnHidden;

    private void OnEnable()
    {
        slidePanel.OnHidden += OnHide;
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= OnHide;
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
    }

    private void Start()
    {
        currentEnergyText.SetPlaceHolderLocalization(constructionEnergyManager);
        nextChargeText.SetPlaceHolderLocalization(constructionEnergyManager);
    }

    private void Update()
    {
        if (!isShowed) return;

        nextChargeText.UpdateText();
    }

    public void Show()
    {
        isShowed = true;
        slidePanel.Show();
        InputStateManager.Instance.AddBlockTarget();

        OnShowed?.Invoke();
    }

    public void Hide()
    {
        slidePanel.Hide();

        OnHidden?.Invoke();
    }

    private void OnHide()
    {
        isShowed = false;
        InputStateManager.Instance.RemoveBlockTarget();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}