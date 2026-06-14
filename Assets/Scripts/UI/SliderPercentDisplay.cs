using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderPercentDisplay : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        UpdateText(slider.value * 100);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void UpdateText(float value)
    {
        int percent = Mathf.RoundToInt(value);
        string text = $"{percent}%";

        this.text.SetText(text);
    }

    private void OnSliderValueChanged(float value)
    {
        UpdateText(value * 100);
    }
}