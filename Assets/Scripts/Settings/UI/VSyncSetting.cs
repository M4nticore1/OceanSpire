using UnityEngine;

public class VSyncSetting : SettingWidget
{
    [SerializeField] private CustomToggle toggle;

    private void OnEnable()
    {
        toggle.OnValueChanged += OnToggleValueChanged;
    }

    private void OnDisable()
    {
        toggle.OnValueChanged -= OnToggleValueChanged;
    }

    private void OnToggleValueChanged(bool value)
    {

    }
}