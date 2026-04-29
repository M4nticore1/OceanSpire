using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ContextMenuElement : UIBehaviour
{
    [SerializeField] private ContextMenu contextMenu;
    [SerializeField] private GameObject content;
    [SerializeField] protected CustomButton button;

    protected override void OnEnable()
    {
        base.OnEnable();

        button.onReleased += OnButtonClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        button.onReleased -= OnButtonClicked;
    }

    protected override void Start()
    {
        base.Start();

        ContextMenuManager.Instance.onContextMenuTargetSelected += OnSelected;
    }

    protected abstract void OnShowed();
    protected abstract void OnButtonClicked();
    protected abstract bool ShouldShow(ContextMenuTarget target);

    protected void Show()
    {
        gameObject.SetActive(true);
        OnShowed();
    }

    protected void Hide()
    {
        gameObject.SetActive(false);
    }

    protected void OnSelected(ContextMenuTarget target)
    {
        if (!target) return;

        if (ShouldShow(target)) {
            Show();
        }
        else {
            Hide();
        }
    }
}