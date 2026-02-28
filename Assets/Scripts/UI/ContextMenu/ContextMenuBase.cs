using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ContextMenuBase : UIBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText = null;

    protected void SetNameText(string name)
    {
        nameText.SetText(name);
    }
}

public abstract class ContextMenuBase<TData> : ContextMenuBase
{
    public abstract void Init(TData data);
}