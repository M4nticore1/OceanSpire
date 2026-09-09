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

    public bool IsShown { get; private set; } = false;

    public event Action OnShowed;
    public event Action OnHidden;

    private void OnEnable()
    {
        slidePanel.OnHidden += OnHide;
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
        constructionEnergyManager.OnEnergyChanged += OnEnergyChanged;
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= OnHide;
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
        constructionEnergyManager.OnEnergyChanged -= OnEnergyChanged;
    }

    private void Start()
    {
        currentEnergyText.SetPlaceHolderLocalization(constructionEnergyManager);
        nextChargeText.SetPlaceHolderLocalization(constructionEnergyManager);
    }

    private void Update()
    {
        if (!IsShown) return;

        nextChargeText.UpdateText();
    }

    public void Show()
    {
        IsShown = true;
        slidePanel.Show();
        InputStateManager.Instance.AddInputBlockTarget(this);

        currentEnergyText.UpdateText();

        OnShowed?.Invoke();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    private void OnHide()
    {
        IsShown = false;
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
    }

    private void OnEnergyChanged(float energy)
    {
        currentEnergyText.UpdateText();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}