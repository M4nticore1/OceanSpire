using System.Linq;
using TMPro;
using UnityEngine;

public class FitSizeToText : FitSizeToContent
{
    protected override void Subscribe()
    {
        base.Subscribe();

        LocalizationManager.Instance.OnLocalizationChanged += OnLocalizationChanged;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        LocalizationManager.Instance.OnLocalizationChanged -= OnLocalizationChanged;
    }

    protected override Vector2 GetSize()
    {
        var tmpText = GetIncludedChildren().Select(child => child.GetComponent<TMP_Text>()).FirstOrDefault(text => text != null);

        if (tmpText == null || string.IsNullOrEmpty(tmpText.text)) {
            return MinSize;
        }

        tmpText.rectTransform.ForceUpdateRectTransforms();
        tmpText.ForceMeshUpdate();

        var size = new Vector2();

        size.x = tmpText.preferredWidth + ExtraSize.x;
        size.y = tmpText.preferredHeight + ExtraSize.y;

        size.x = Mathf.Max(size.x, MinSize.x);
        size.y = Mathf.Max(size.y, MinSize.y);

        return size;
    }

    private void OnLocalizationChanged()
    {
        RunUpdateSizeEndOfFrame();
    }
}