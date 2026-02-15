using TMPro;
using UnityEngine;

public class TextLocalizer : MonoBehaviour
{
    private TextMeshProUGUI textBlock;
    [SerializeField] private LocalizationItem item = null;

    private void Awake()
    {
        textBlock = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLocalizationChanged += ChangeLocalization;
        if (item && LocalizationManager.Instance.GetLocalizationEntry(item) != null) {
            ChangeLocalization();
        }
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLocalizationChanged -= ChangeLocalization;
    }

    public void SetLocalizationItem(LocalizationItem item)
    {
        this.item = item;

        ChangeLocalization();
    }

    private void ChangeLocalization()
    {
        if (!item) {
            Debug.LogError($"item is not valid in object {gameObject}.");
            return;
        }

        LocalizationEntry localization = LocalizationManager.Instance.GetLocalizationEntry(item);

        if (localization != null) {
            textBlock.SetText(localization.Value);

            int fontIndex = localization.FontIndex;
            TMP_FontAsset[] fonts = LocalizationManager.Instance.currentLocalization.Fonts;
            if (fonts != null) {
                if (fonts.Length > fontIndex) {
                    TMP_FontAsset font = fonts[localization.FontIndex];

                    if (font != null) {
                        textBlock.font = font;
                    }
                }
                else {
                    Debug.LogError($"The length of fonts array is less than font index of '{localization.Item.name}' item.");
                }
            }
            else {
                Debug.LogError($"The fonts array is not valid.");
            }
        }
    }
}
