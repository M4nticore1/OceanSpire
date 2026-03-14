using System;
using TMPro;
using UnityEngine;

public class CreateNewWorldMenu : MonoBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private CustomButton createWorldButton = null;
    [SerializeField] private TextMeshProUGUI worldNameAlreadyExistsText = null;

    public event Action onClosed;

    private void OnEnable()
    {
        slidePanel.onClosed += OnClosed;
        inputField.onValueChanged.AddListener(OnWorldNameInputFieldChangeValue);
        createWorldButton.onReleased += OnCreateWorldButtonClicked;
    }

    private void OnDisable()
    {
        slidePanel.onClosed -= OnClosed;
        inputField.onValueChanged.RemoveListener(OnWorldNameInputFieldChangeValue);
        createWorldButton.onReleased -= OnCreateWorldButtonClicked;
    }

    private void Start()
    {
        inputField.onFocusSelectAll = false;
    }

    public void Open()
    {
        slidePanel.Open();

        inputField.text = "";
        worldNameAlreadyExistsText.gameObject.SetActive(false);
        string name = inputField.text;
        CheckWorldName(name);
    }

    public void Close()
    {
        slidePanel.Close();
        OnClosed();
    }

    private void OnClosed()
    {
        onClosed?.Invoke();
    }

    private void OnWorldNameInputFieldChangeValue(string value)
    {
        CheckWorldName(value);
    }

    private void OnCreateWorldButtonClicked()
    {
        string worldName = inputField.text;
        WorldSaveManager.Instance.CreateWorld(worldName);
    }

    private void CheckWorldName(string name)
    {
        if (!IsWorldNameFit(name)) {
            createWorldButton.SetState(CustomButtonState.Disabled);
            return;
        }

        if (IsWorldNameExist(name)) {
            worldNameAlreadyExistsText.gameObject.SetActive(true);
            createWorldButton.SetState(CustomButtonState.Disabled);
            return;
        }

        worldNameAlreadyExistsText.gameObject.SetActive(false);
        createWorldButton.SetState(CustomButtonState.Idle);
    }

    private bool IsWorldNameExist(string name)
    {
        WorldData[] worldData = WorldSaveManager.Instance.allSaveData;
        if (worldData == null) return false;

        foreach (var data in worldData) {
            if (data != null && data.cityData.cityName == name) {
                return true;
            }
        }
        return false;
    }

    private bool IsWorldNameFit(string name)
    {
        if (name.Length > 0) {
            return true;
        }
        return false;
    }
}
