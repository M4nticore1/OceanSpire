using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectedDisplay : UIBehaviour
{
    [SerializeField] private GameObject content;

    private bool isSubscribed = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        Subscribe();
        TryDisplay(SelectManager.Instance?.SelectedComponent);
        TryHide(SelectManager.Instance?.SelectedComponent);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Unsubscribe();
    }

    protected override void Start()
    {
        base.Start();

        Subscribe();
    }

    protected virtual void Subscribe()
    {
        if (!ShouldSubscribe()) return;

        SelectManager.Instance.OnComponentSelected += OnComponentSelected;
        SelectManager.Instance.OnComponentDeselected += OnComponentDeselected;

        isSubscribed = true;
    }

    protected virtual void Unsubscribe()
    {
        if (!ShouldUnsubscribe()) return;

        SelectManager.Instance.OnComponentSelected -= OnComponentSelected;
        SelectManager.Instance.OnComponentDeselected -= OnComponentDeselected;

        isSubscribed = false;
    }

    protected virtual bool ShouldSubscribe()
    {
        if (isSubscribed) return false;
        if (!SelectManager.Instance) return false;

        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        if (!isSubscribed) return false;
        if (!SelectManager.Instance) return false;

        return true;
    }

    protected virtual void Display(SelectComponent selectComponent)
    {
        if (content) {
            content.SetActive(true);
        }
    }

    protected virtual void Hide(SelectComponent selectComponent)
    {
        if (content) {
            content.SetActive(false);
        }
    }

    protected abstract bool ShouldDisplay(SelectComponent selectComponent);

    private void TryDisplay(SelectComponent selectComponent)
    {
        if (!ShouldDisplay(selectComponent)) return;

        Display(selectComponent);
    }

    private void TryHide(SelectComponent selectComponent)
    {
        if (ShouldDisplay(selectComponent)) return;

        Hide(selectComponent);
    }

    private void OnComponentSelected(SelectComponent selectComponent)
    {
        TryHide(selectComponent);
        TryDisplay(selectComponent);
    }

    private void OnComponentDeselected(SelectComponent selectselectComponentd)
    {
        TryHide(selectselectComponentd);
    }
}