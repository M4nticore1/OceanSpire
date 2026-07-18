using System;
using UnityEngine;

public class PlayerSettingsMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private CustomButton closeButton;

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
        gameObject.SetActive(true);
        InputStateManager.Instance.SetGameplayInputBlocked(true);

        OnShown?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        InputStateManager.Instance.SetGameplayInputBlocked(false);

        OnHidden?.Invoke();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}
