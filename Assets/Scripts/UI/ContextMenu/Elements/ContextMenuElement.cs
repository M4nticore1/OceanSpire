using UnityEngine;

public abstract class ContextMenuElement : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] protected CustomButton button;

    private void Awake()
    {
        SelectManager.onComponentSelected += OnSelected;
    }

    private void OnEnable()
    {
        button.onReleased += OnButtonClicked;
    }

    private void OnDisable()
    {
        button.onReleased -= OnButtonClicked;
    }

    protected abstract void OnShowed();
    protected abstract void OnButtonClicked();
    protected abstract bool ShouldShow();

    protected void Show()
    {
        gameObject.SetActive(true);
        OnShowed();
    }

    protected void Hide()
    {
        gameObject.SetActive(false);
    }

    protected void OnSelected(SelectComponent selected)
    {
        if (ShouldShow()) {
            Show();
        }
        else {
            Hide();
        }
    }
}