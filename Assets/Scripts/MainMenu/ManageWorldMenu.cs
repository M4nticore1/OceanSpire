using System;
using TMPro;
using UnityEngine;

public abstract class ManageWorldMenu : MonoBehaviour
{
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private KeyboardOffsetUI keyboardOffsetUI;

    [SerializeField] private TMP_InputField inputField;
    protected TMP_InputField InputField => inputField;

    [SerializeField] private CustomButton actionButton;
    [SerializeField] private CustomButton cancelButton;

    [SerializeField] private InputFieldValidatorsManager inputFieldValidatorsManager;

    protected WorldData worldData { get; private set; }

    public event Action OnShown;
    public event Action OnHidden;

    protected virtual void Awake()
    {
        inputField.onFocusSelectAll = false;
    }

    private void OnEnable()
    {
        slidePanel.OnHidden += HandleHidden;
        inputField.onValueChanged.AddListener(HandleInputFieldValueChanged);

        actionButton.OnReleased.AddListener(HandleActionButtonClicked);
        cancelButton.OnReleased.AddListener(HandleCancelButtonClicked);
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= HandleHidden;
        inputField.onValueChanged.RemoveListener(HandleInputFieldValueChanged);

        actionButton.OnReleased.RemoveListener(HandleActionButtonClicked);
        cancelButton.OnReleased.RemoveListener(HandleCancelButtonClicked);
    }

    public void Show()
    {
        HandleShown();
    }

    public void Show(WorldData worldData)
    {
        if (worldData == null) {
            Debug.LogError($"[{nameof(RenameWorldMenu)}] WorldData is not valid!");
            return;
        }

        this.worldData = worldData;

        Show();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    protected virtual void HandleShown()
    {
        slidePanel.Show();
        keyboardOffsetUI.SetClosable(false);

        UpdateActionButtonEnabled();
        UpdateWorldNameValidators();

        OnShown?.Invoke();
    }

    protected virtual void HandleHidden()
    {
        keyboardOffsetUI.SetClosable(true);

        OnHidden?.Invoke();
    }

    protected virtual void HandleActionButtonClicked()
    {
        Hide();
    }

    private void UpdateActionButtonEnabled()
    {
        var invalidLength = inputField.text.Length <= 0 ? true : false;
        var invalidName = inputFieldValidatorsManager.gameObject.activeInHierarchy ? inputFieldValidatorsManager.HasInvalid : false;
        var sameName = worldData != null ? inputField.text == worldData.WorldName : false;

        actionButton.SetState(invalidLength || invalidName || sameName ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void UpdateWorldNameValidators()
    {
        if (inputField.text.Length > 0 && (worldData != null ? inputField.text != worldData.WorldName : false)) {
            inputFieldValidatorsManager.gameObject.SetActive(true);
            inputFieldValidatorsManager.UpdateValidatorsShown(inputField.text);
        }
        else {
            inputFieldValidatorsManager.gameObject.SetActive(false);
        }
    }

    private void HandleInputFieldValueChanged(string value)
    {
        UpdateActionButtonEnabled();
        UpdateWorldNameValidators();
    }

    private void HandleCancelButtonClicked()
    {
        Hide();
    }
}