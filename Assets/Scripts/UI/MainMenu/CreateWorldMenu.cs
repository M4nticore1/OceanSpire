using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateWorldMenu : MonoBehaviour
{
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private KeyboardOffsetUI keyboardOffsetUI;
    [SerializeField] private CustomButton createWorldButton;
    [SerializeField] private CustomButton cancelButton;
    [SerializeField] private InputFieldValidatorsManager inputFieldValidatorsManager;

    [Header("World Name")]
    [SerializeField] private TextLocalizer incorrectWorldNameText;
    [SerializeField] private LocalizationItem existWorldNameLocalization;
    [SerializeField] private LocalizationItem incorrectWorldNameLocalization;

    public event Action OnClosed;

    private void OnEnable()
    {
        slidePanel.OnHidden += HandleClosed;
        inputField.onValueChanged.AddListener(HandleWorldNameInputFieldChangeValue);
        createWorldButton.OnReleased.AddListener(HandleCreateWorldButtonClicked);
        cancelButton.OnReleased.AddListener(HandleCancelButtonClicked);
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= HandleClosed;
        inputField.onValueChanged.RemoveListener(HandleWorldNameInputFieldChangeValue);
        createWorldButton.OnReleased.RemoveListener(HandleCreateWorldButtonClicked);
        cancelButton.OnReleased.RemoveListener(HandleCancelButtonClicked);
    }

    private void Start()
    {
        inputField.onFocusSelectAll = false;
    }

    public void Open()
    {
        keyboardOffsetUI.SetClosable(false);
        slidePanel.Show();

        inputField.text = "";

        UpdateCreateButtonEnabled();
        StartCoroutine(UpdateValidatorsShownEndOfFrame());
    }

    public void Close()
    {
        slidePanel.Hide();
        HandleClosed();
    }

    private void UpdateCreateButtonEnabled()
    {
        var worldNameLength = inputField.text.Length;
        var invalidLength = worldNameLength <= 0 || worldNameLength >= 32 ? true : false;
        var invalid = inputFieldValidatorsManager.HasInvalid;

        createWorldButton.SetState(invalidLength || invalid ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void UpdateValidatorsShown()
    {
        if (inputField.text.Length <= 0) {
            inputFieldValidatorsManager.HideValidators();
        }
    }

    private void HandleClosed()
    {
        keyboardOffsetUI.SetClosable(true);
        OnClosed?.Invoke();
    }

    private void HandleCreateWorldButtonClicked()
    {
        var worldName = inputField.text;

        WorldSaveHandler.Instance.SetSaveWorldName(worldName);
        SceneManager.LoadScene(1);
    }

    private void HandleCancelButtonClicked()
    {
        Close();
    }

    private void HandleWorldNameInputFieldChangeValue(string value)
    {
        UpdateCreateButtonEnabled();
        StartCoroutine(UpdateValidatorsShownEndOfFrame());
    }

    private IEnumerator UpdateValidatorsShownEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        UpdateValidatorsShown();
    }
}