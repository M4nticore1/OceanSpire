using TMPro;
using UnityEngine;

public class RenameWorldMenu : MonoBehaviour
{
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private CustomButton renameButton;
    [SerializeField] private CustomButton cancelButton;
    [SerializeField] private InputFieldValidatorsManager InputFieldValidatorsManager;

    private WorldData worldData;

    private void OnEnable()
    {
        inputField.onValueChanged.AddListener(HandleInputFieldValueChanged);
        renameButton.OnReleased.AddListener(HandleRenameButtonClicked);
        cancelButton.OnReleased.AddListener(HandleCancelButtonClicked);
    }

    private void OnDisable()
    {
        inputField.onValueChanged.RemoveListener(HandleInputFieldValueChanged);
        renameButton.OnReleased.RemoveListener(HandleRenameButtonClicked);
        cancelButton.OnReleased.RemoveListener(HandleCancelButtonClicked);
    }

    public void Show(WorldData worldData)
    {
        if (worldData == null) {
            Debug.LogError($"[{nameof(RenameWorldMenu)}] WorldData is not valid!");
            return;
        }

        this.worldData = worldData;
        inputField.text = worldData.WorldName;

        UpdateRenameButtonEnabled();
        UpdateWorldNameValidators();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    private void UpdateRenameButtonEnabled()
    {
        if (worldData == null)
            return;

        renameButton.SetState(worldData.WorldName == inputField.text ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void UpdateWorldNameValidators()
    {
        if (worldData == null)
            return;

        InputFieldValidatorsManager.UpdateValidatorsShown(worldData.WorldName);
    }

    private void HandleInputFieldValueChanged(string value)
    {
        UpdateRenameButtonEnabled();
        UpdateWorldNameValidators();
    }

    private void HandleRenameButtonClicked()
    {

    }

    private void HandleCancelButtonClicked()
    {
        Hide();
    }
}