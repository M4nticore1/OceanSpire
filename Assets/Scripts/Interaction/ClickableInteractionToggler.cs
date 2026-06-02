using System.Collections.Generic;
using UnityEngine;

public class ClickableInteractionToggler : InteractionToggler
{
    private Dictionary<IClickable, bool> selectionsDict = new();

    public override void EnableInteraction()
    {
        foreach (var select in selectionsDict) {
            select.Key.SetClickable(select.Value);
        }

        selectionsDict.Clear();
    }

    public override void DisableInteraction()
    {
        var monobehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var monobehaviour in monobehaviours) {
            var clickable = monobehaviour.GetComponent<IClickable>();
            if (clickable == null) continue;

            if (!selectionsDict.TryAdd(clickable, clickable.IsClickable)) continue;

            clickable.SetClickable(false);
        }
    }
}