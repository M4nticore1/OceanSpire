using System;
using System.Collections.Generic;
using System.Linq;
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
    private ILocalizable placeHoldersLocalization;

    private void Awake()
    {
        textBlock = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLocalizationChanged += OnLocalizationChanged;
        UpdateText();
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLocalizationChanged -= OnLocalizationChanged;
    }

    public void UpdateText()
    {
        if (item) {
            string text = LocalizationManager.Instance.GetText(item);

            if (placeHoldersLocalization != null) {
                Dictionary<string, string> dict = placeHoldersLocalization.GetLocalizations();

                foreach (var key in dict.Keys.ToArray()) {
                    string holder = "{" + key + "}";
                    string value = dict[key];
                    text = text.Replace(holder, value);
                }
            }

            SetText(text);

            TMP_FontAsset font = LocalizationManager.Instance.GetFont(textRole);
            SetFont(font);
        }
        else {
            SetText(null);
        }
    }

    public void SetLocalizationItem(LocalizationItem item)
    {
        this.item = item;
    }

    public void SetPlaceHolderLocalization(ILocalizable placeHolders)
    {
        placeHoldersLocalization = placeHolders;
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
        UpdateText();
    }
}
