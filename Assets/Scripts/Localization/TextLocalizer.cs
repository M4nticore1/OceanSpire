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

    [SerializeField] private TextRole textRole = TextRole.Default;
    [SerializeField] private LocalizationItem item;

    [SerializeField] private MonoBehaviour localizationTarget;
    private ILocalizable LocalizationTarget = null;

    private void Awake()
    {
        textBlock = GetComponent<TextMeshProUGUI>();

        if (localizationTarget) {
            SetPlaceHolderLocalization(localizationTarget as ILocalizable);
        }
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
            if (text == "") return;

            if (LocalizationTarget != null) {
                Dictionary<string, string> dict = LocalizationTarget.GetLocalization();

                foreach (var key in dict.Keys.ToArray()) {
                    string holder = "{" + key + "}";
                    string value = dict[key];
                    text = text.Replace(holder, value);
                }
            }

            SetText(text);
        }

        UpdateFont();
    }

    public void SetLocalizationItem(LocalizationItem item)
    {
        this.item = item;
    }

    public void SetPlaceHolderLocalization(ILocalizable placeHolders)
    {
        LocalizationTarget = placeHolders;
    }

    public void SetText(string text)
    {
        textBlock.SetText(text);
    }

    private void UpdateFont()
    {
        TMP_FontAsset font = LocalizationManager.Instance.GetFont(textRole);
        SetFont(font);
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