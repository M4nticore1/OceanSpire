using System;
using TMPro;
using UnityEngine;

public class InputFieldValidatorsManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private InputFieldValidator[] validators;

    public bool HasInvalid { get; private set; } = false;

    private event Action<InputFieldValidator> OnValidatorShown;

    private void OnEnable()
    {
        inputField.onValueChanged.AddListener(HandleInputFieldValueChanged);
        UpdateValidatorsShown(inputField.text);
    }

    private void OnDisable()
    {
        inputField.onValueChanged.RemoveListener(HandleInputFieldValueChanged);
    }

    public void UpdateValidatorsShown(string text)
    {
        var invalid = false;

        foreach (var validator in validators) {
            if (validator == null) continue;

            if (invalid) {
                validator.Hide();
            }
            else {
                validator.UpdateShown(text);
            }

            if (validator.IsShown) {
                invalid = true;
                OnValidatorShown?.Invoke(validator);
            }
        }

        HasInvalid = invalid;
    }

    private void HandleInputFieldValueChanged(string value)
    {
        UpdateValidatorsShown(value);
    }
}