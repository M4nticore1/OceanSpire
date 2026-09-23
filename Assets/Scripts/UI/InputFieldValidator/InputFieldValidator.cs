using UnityEngine;

public abstract class InputFieldValidator : MonoBehaviour
{
    public bool IsShown => gameObject.activeInHierarchy;

    public void UpdateShown(string text)
    {
        if (IsValid(text)) {
            Hide();
        }
        else {
            Show();
        }
    }

    protected abstract bool IsValid(string text);

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}