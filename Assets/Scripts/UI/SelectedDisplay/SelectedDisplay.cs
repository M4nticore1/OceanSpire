using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectedDisplay : UIBehaviour
{
    [SerializeField] private GameObject content;

    private bool isSubscribed = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        TrySubscribe();
        TryDisplay(SelectManager.Instance?.SelectedComponent);
        TryHide(SelectManager.Instance?.SelectedComponent);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        TryUnsubscribe();
    }

    protected override void Start()
    {
        base.Start();

        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!SelectManager.Instance) return;

        SelectManager.Instance.OnComponentSelected += OnComponentSelected;
        SelectManager.Instance.OnComponentDeselected += OnComponentDeselected;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!SelectManager.Instance) return;

        SelectManager.Instance.OnComponentSelected -= OnComponentSelected;
        SelectManager.Instance.OnComponentDeselected -= OnComponentDeselected;

        isSubscribed = false;
    }

    protected abstract bool ShouldDisplay(SelectComponent selectComponent);

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