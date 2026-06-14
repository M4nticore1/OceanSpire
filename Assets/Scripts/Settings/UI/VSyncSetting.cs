using UnityEngine;

public class VSyncSetting : MonoBehaviour
{
    [SerializeField] private PlayerSettingsManager playerSettingsManager;
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