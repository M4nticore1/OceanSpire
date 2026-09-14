using System.Linq;
using TMPro;
using UnityEngine;

public class FitSizeToText : FitSizeToContent
{
    protected override float GetHeight()
    {
        var tmpText = GetIncludedChildren().Select(child => child.GetComponent<TMP_Text>()).FirstOrDefault(text => text != null);

        if (tmpText == null || string.IsNullOrEmpty(tmpText.text)) {
            return MinHeight;
        }

        tmpText.ForceMeshUpdate();
        var requiredHeight = tmpText.preferredHeight + ExtraHeight;

        return Mathf.Max(requiredHeight, MinHeight);
    }
}