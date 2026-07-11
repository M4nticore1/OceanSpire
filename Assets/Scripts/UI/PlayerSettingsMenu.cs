using UnityEngine;

public class PlayerSettingsMenu : MonoBehaviour
{
    [SerializeField] private CustomButton closeButton;

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
        gameObject.SetActive(true);
        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}
