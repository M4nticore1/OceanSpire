using System.Collections.Generic;
using UnityEngine;

public class SelectableGroup : MonoBehaviour
{
    [SerializeField] private List<CustomSelectable> selectables = new List<CustomSelectable>();
    private CustomSelectable selectedSelectable;

    private void Start()
    {
        foreach (var selectable in selectables) {
            selectable.SetSelectableGroup(this);
        }
    }

    public void SelectSelectable(CustomSelectable selectable)
    {
        if (selectedSelectable) {
            selectedSelectable.Deselect();
        }

        selectable.Select();
    }
}
