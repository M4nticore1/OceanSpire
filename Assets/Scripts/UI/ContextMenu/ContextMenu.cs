using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ContextMenu : UIBehaviour
{
    [SerializeField] private TextLocalizer nameTextLocalizer = null;
    [SerializeField] private LayoutGroup layoutGroup = null;

    protected void SetNameLocalization(LocalizationItem localization)
    {
        nameTextLocalizer.SetLocalizationItem(localization);
    }

    protected GameObject CreatePanel(GameObject panel)
    {
        GameObject spawned = Instantiate(panel, layoutGroup.transform);
        spawned.transform.SetAsLastSibling();
        return spawned;
    }
}

public abstract class ContextMenu<TData> : ContextMenu
{
    public abstract void Init(TData data);
}