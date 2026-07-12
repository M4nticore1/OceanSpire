using UnityEngine;

public class BuilderEnergyMenu : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private BuilderEnergyManager constructionEnergyManager;

    [Header("UI")]
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private TextLocalizer currentEnergyText;
    [SerializeField] private TextLocalizer nextChargeText;

    private bool isShowed;

    private void OnEnable()
    {
        slidePanel.OnClosed += OnHide;
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        slidePanel.OnClosed -= OnHide;
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
        slidePanel.Open();
        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Hide()
    {
        slidePanel.Close();
    }

    private void OnHide()
    {
        isShowed = false;
        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}