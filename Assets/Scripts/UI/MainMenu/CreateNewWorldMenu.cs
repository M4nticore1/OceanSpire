using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateNewWorldMenu : MonoBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private KeyboardOffsetUI keyboardOffsetUI;
    [SerializeField] private CustomButton createWorldButton;
    [SerializeField] private CustomButton cancelButton;

    [Header("World Name")]
    [SerializeField] private TextLocalizer incorrectWorkNameText;
    [SerializeField] private LocalizationItem existWorldNameLocalization;
    [SerializeField] private LocalizationItem incorrectWorldNameLocalization;

    public event Action onClosed;

    private void OnEnable()
    {
        slidePanel.OnClosed += OnClosed;
        inputField.onValueChanged.AddListener(OnWorldNameInputFieldChangeValue);
        createWorldButton.OnReleased.AddListener(OnCreateWorldButtonClicked);
        cancelButton.OnReleased.AddListener(OnCancelButtonClicked);
    }

    private void OnDisable()
    {
        slidePanel.OnClosed -= OnClosed;
        inputField.onValueChanged.RemoveListener(OnWorldNameInputFieldChangeValue);
        createWorldButton.OnReleased.RemoveListener(OnCreateWorldButtonClicked);
        cancelButton.OnReleased.RemoveListener(OnCancelButtonClicked);
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
        string name = inputField.text;
        CheckWorldName(name);
    }

    public void Close()
    {
        slidePanel.Hide();
        OnClosed();
    }

    private void OnClosed()
    {
        keyboardOffsetUI.SetClosable(true);
        onClosed?.Invoke();
    }

    private void OnWorldNameInputFieldChangeValue(string value)
    {
        CheckWorldName(value);
    }

    private void OnCreateWorldButtonClicked()
    {
        string worldName = inputField.text;

        WorldSaveHandler.Instance.SetSaveWorldName(worldName);
        SceneManager.LoadScene(1);
    }

    private void OnCancelButtonClicked()
    {
        Close();
    }

    private void CheckWorldName(string name)
    {
        if (!IsPossibleWorldNameLength(name)) {
            incorrectWorkNameText.gameObject.SetActive(false);
            createWorldButton.SetState(CustomButtonState.Disabled);
            return;
        }

        if (!IsPossibleWorldName(name)) {
            incorrectWorkNameText.gameObject.SetActive(true);
            incorrectWorkNameText.SetLocalizationItem(incorrectWorldNameLocalization);
            createWorldButton.SetState(CustomButtonState.Disabled);
            return;
        }

        if (IsWorldNameExist(name)) {
            incorrectWorkNameText.gameObject.SetActive(true);
            incorrectWorkNameText.SetLocalizationItem(existWorldNameLocalization);
            createWorldButton.SetState(CustomButtonState.Disabled);
            return;
        }

        incorrectWorkNameText.gameObject.SetActive(false);
        createWorldButton.SetState(CustomButtonState.Idle);
    }

    private bool IsPossibleWorldName(string name)
    {
        if (!WorldSaveSystem.CanCreateSaveFolder(name)) return false;

        return true;
    }

    private bool IsWorldNameExist(string name)
    {
        var worldData = WorldSaveHandler.Instance.AllSaveData;
        if (worldData == null) return false;

        foreach (var data in worldData) {
            if (data != null && data.WorldName == name) {
                return true;
            }
        }
        return false;
    }

    private bool IsPossibleWorldNameLength(string name)
    {
        if (name.Length <= 0) return false;

        return true;
    }
}