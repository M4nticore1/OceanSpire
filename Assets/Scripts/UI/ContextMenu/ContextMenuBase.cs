using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ContextMenuBase : UIBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private LayoutGroup layoutGroup = null;

    protected void SetNameText(string name)
    {
        nameText.SetText(name);
    }

    protected GameObject CreatePanel(GameObject panel)
    {
        GameObject spawned = Instantiate(panel, layoutGroup.transform);
        spawned.transform.SetAsLastSibling();
        return spawned;
    }
}

public abstract class ContextMenuBase<TData> : ContextMenuBase
{
    public abstract void Init(TData data);
}