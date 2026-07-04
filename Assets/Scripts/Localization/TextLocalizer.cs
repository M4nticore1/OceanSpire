using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public enum TextRole
{
    Default,
    Title
}

public class TextLocalizer : UIBehaviour
{
    private TextMeshProUGUI textBlock;
    public TextMeshProUGUI TextBlock => textBlock ? textBlock : GetComponent<TextMeshProUGUI>();

    [SerializeField] private TextRole textRole = TextRole.Default;
    public TextRole TextRole => textRole;

    [SerializeField] private LocalizationItem item;
    public LocalizationItem Item => item;

    [SerializeField] private MonoBehaviour localizationTarget;
    private ILocalizable LocalizationTarget = null;

    private bool updateText = false;

    protected override void Awake()
    {
        base.Awake();

        textBlock = GetComponent<TextMeshProUGUI>();

        if (localizationTarget) {
            SetPlaceHolderLocalization(localizationTarget as ILocalizable);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LocalizationManager.Instance.OnLocalizationChanged += OnLocalizationChanged;
        UpdateText();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        LocalizationManager.Instance.OnLocalizationChanged -= OnLocalizationChanged;
    }

    public void SetLocalizationItem(LocalizationItem item)
    {
        this.item = item;
        UpdateText();
    }

    public void SetPlaceHolderLocalization(ILocalizable placeHolders)
    {
        LocalizationTarget = placeHolders;
        UpdateText();
    }

    public void SetText(string text)
    {
        TextBlock.SetText(text);
    }

    public void UpdateText()
    {
        if (item) {
            string text = LocalizationManager.Instance.GetText(item);
            if (text == null) return;
            if (text == "") return;

            if (LocalizationTarget != null) {
                var dict = LocalizationTarget.GetLocalization();
                if (dict == null) return;

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

    private void UpdateFont()
    {
        var font = LocalizationManager.Instance.GetFont(textRole);
        SetFont(font);
    }

    private void SetFont(TMP_FontAsset font)
    {
        TextBlock.font = font;
    }

    private void OnLocalizationChanged()
    {
        UpdateText();
    }
}