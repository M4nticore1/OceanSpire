using System;
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
    public TextMeshProUGUI TextBlock => textBlock ? textBlock : GetComponent<TextMeshProUGUI>();

    [SerializeField] private TextRole textRole = TextRole.Default;
    public TextRole TextRole => textRole;

    [SerializeField] private LocalizationItem item;
    public LocalizationItem Item => item;

    [SerializeField] private MonoBehaviour localizationTarget;
    private ILocalizable LocalizationTarget = null;

    private LocalizationManager localizationManager => LocalizationManager.Instance;

    private void Awake()
    {
        textBlock = GetComponent<TextMeshProUGUI>();

        if (localizationTarget) {
            SetPlaceHolderLocalization(localizationTarget as ILocalizable);
        }
    }

    private void OnEnable()
    {
        if (localizationManager != null) {
            localizationManager.OnLocalizationChanged += OnLocalizationChanged;
        }

        UpdateText();
    }

    private void OnDisable()
    {
        if (localizationManager != null) {
            localizationManager.OnLocalizationChanged -= OnLocalizationChanged;
        }
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
        if (localizationManager == null) return;

        if (item) {
            string text = localizationManager.GetLocalizedText(item);
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
        if (localizationManager == null) return;

        var font = localizationManager.GetFont(textRole);
        SetFont(font);
    }

    private void SetFont(TMP_FontAsset font)
    {
        if (!font) {
            Debug.LogError($"[{nameof(TextLocalizer)}] Font is not valid!");
            return;
        }

        if (TextBlock.font != font) {
            TextBlock.font = font;
            TextBlock.SetAllDirty();
            TextBlock.ForceMeshUpdate();
        }
    }

    private void OnLocalizationChanged()
    {
        UpdateText();
    }
}