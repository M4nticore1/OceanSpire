using UnityEngine;

public class TimeFormatterSymbolsController : MonoBehaviour
{
    [SerializeField] private LocalizationItem secondShortNameLocalization;
    [SerializeField] private LocalizationItem minuteShortNameLocalization;
    [SerializeField] private LocalizationItem hourShortNameLocalization;

    private string HourShortName => LocalizationManager.Instance.GetLocalizedText(hourShortNameLocalization);
    private string MinutehortName => LocalizationManager.Instance.GetLocalizedText(minuteShortNameLocalization);
    private string SecondShortName => LocalizationManager.Instance.GetLocalizedText(secondShortNameLocalization);

    private void Awake()
    {
        UpdateSymbols();
    }

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLocalizationChanged += OnLocalizationChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLocalizationChanged -= OnLocalizationChanged;
    }

    private void Start()
    {
        UpdateSymbols();
    }

    private void UpdateSymbols()
    {
        var symbols = new TimeSymbols()
        {
            Hour = HourShortName,
            Minute = MinutehortName,
            Second = SecondShortName
        };

        TimeFormatter.SetSymbols(symbols);
    }

    private void OnLocalizationChanged()
    {
        UpdateSymbols();
    }
}