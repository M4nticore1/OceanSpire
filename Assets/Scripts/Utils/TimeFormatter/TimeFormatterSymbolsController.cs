using UnityEngine;

public class TimeFormatterSymbolsController : MonoBehaviour
{
    [SerializeField] private LocalizationItem secondShortNameLocalization;
    [SerializeField] private LocalizationItem minuteShortNameLocalization;
    [SerializeField] private LocalizationItem hourShortNameLocalization;

    private string HourShortName => LocalizationManager.Instance.GetText(hourShortNameLocalization);
    private string MinutehortName => LocalizationManager.Instance.GetText(minuteShortNameLocalization);
    private string SecondShortName => LocalizationManager.Instance.GetText(secondShortNameLocalization);

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLocalizationChanged += OnLocalizationChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLocalizationChanged -= OnLocalizationChanged;
    }

    private void OnLocalizationChanged()
    {
        var symbols = new TimeSymbols()
        {
            Hour = HourShortName,
            Minute = MinutehortName,
            Second = SecondShortName
        };

        TimeFormatter.SetSymbols(symbols);
    }
}