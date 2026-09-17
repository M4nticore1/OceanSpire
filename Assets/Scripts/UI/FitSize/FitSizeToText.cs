using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FitSizeToText : FitSizeToContent
{
    protected override float GetHeight()
    {
        var tmpText = GetIncludedChildren().Select(child => child.GetComponent<TMP_Text>()).FirstOrDefault(text => text != null);

        if (tmpText == null || string.IsNullOrEmpty(tmpText.text)) {
            return MinHeight;
        }

        tmpText.rectTransform.ForceUpdateRectTransforms();
        tmpText.ForceMeshUpdate();

        var requiredHeight = tmpText.preferredHeight + ExtraHeight;
        float finalHeight = Mathf.Max(requiredHeight, MinHeight);

        return finalHeight;
    }
}