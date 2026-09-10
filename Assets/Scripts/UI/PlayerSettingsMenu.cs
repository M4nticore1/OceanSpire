using System;
using UnityEngine;

public class PlayerSettingsMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private CustomButton closeButton;

    public bool IsShown { get; private set; } = false;

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
        IsShown = true;
        gameObject.SetActive(true);
        InputStateManager.Instance.AddInputBlockTarget(this);

        OnShown?.Invoke();
    }

    public void Hide()
    {
        IsShown = false;
        gameObject.SetActive(false);
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}
