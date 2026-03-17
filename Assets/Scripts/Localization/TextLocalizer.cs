using System;
using TMPro;
using UnityEngine;

[Serializable]
public enum TextRole
{
    Default,
    Title
}

public class TextLocalizer : MonoBehaviour
{
    private TextMeshProUGUI textBlock;
    [SerializeField] private LocalizationItem item;
    [SerializeField] private TextRole textRole = TextRole.Default;

    private void Awake()
    {
        textBlock = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLocalizationChanged += OnLocalizationChanged;
        AssignLocalization();
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLocalizationChanged -= OnLocalizationChanged;
    }

    public void SetLocalizationItem(LocalizationItem item)
    {
        this.item = item;
        AssignLocalization();
    }

    private void AssignLocalization()
    {
        if (!item) return;

        string text = LocalizationManager.Instance.GetText(item);
        SetText(text);

        TMP_FontAsset font = LocalizationManager.Instance.GetFont(textRole);
        SetFont(font);
    }

    private void SetText(string text)
    {
        textBlock.SetText(text);
    }

    private void SetFont(TMP_FontAsset font)
    {
        textBlock.font = font;
    }

    private void OnLocalizationChanged()
    {
        AssignLocalization();
    }
}
