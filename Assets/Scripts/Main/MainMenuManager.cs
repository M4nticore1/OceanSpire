using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField newWorldNameInputField = null;
    [SerializeField] private CustomSelectable createWorldButton = null;
    [SerializeField] private CustomSelectable loadSaveButton = null;
    [SerializeField] private CustomSelectable deleteSaveButton = null;
    [SerializeField] private TextMeshProUGUI worldNameAlreadyExistsTextBlock = null;

    public SaveSlotWidget selectedWorldSaveSlot;
    [SerializeField] private SlidePanel createWorldSlidePanel = null;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        SaveSlotWidget.OnSaveSlotSelected += OnSlotSelected;
        SaveSlotWidget.OnSaveSlotDeselected += OnSlotDeselected;
        createWorldSlidePanel.onOpened += OnCreateWorldMenuOpen;
        createWorldSlidePanel.onClosed += OnCreateWorldMenuClose;
        createWorldButton.onReleased += CreateWorld;
        loadSaveButton.onReleased += LoadSave;
        deleteSaveButton.onReleased += DeleteSave;
        newWorldNameInputField.onValueChanged.AddListener(OnWorldNameInputFieldChangeValue);
    }

    private void OnDisable()
    {
        SaveSlotWidget.OnSaveSlotSelected -= OnSlotSelected;
        SaveSlotWidget.OnSaveSlotDeselected -= OnSlotDeselected;
        createWorldSlidePanel.onOpened -= OnCreateWorldMenuOpen;
        createWorldSlidePanel.onClosed -= OnCreateWorldMenuClose;
        createWorldButton.onReleased -= CreateWorld;
        loadSaveButton.onReleased -= LoadSave;
        deleteSaveButton.onReleased -= DeleteSave;
        newWorldNameInputField.onValueChanged.AddListener(OnWorldNameInputFieldChangeValue);
    }

    private void CreateWorld()
    {
        string worldName = newWorldNameInputField.text;
        SaveData data = SaveSystem.GetSaveDataByWorldName(worldName);
        if (data != null) {

        }
        else {
            SaveManager.Instance.SetSaveWorldName(worldName);
            SceneManager.LoadScene(1);
        }
    }

    private void LoadSave()
    {
        Debug.Log("Load");
        SaveData data = selectedWorldSaveSlot.worldSaveData;
        SaveManager.Instance.SetSaveData(data);
        SceneManager.LoadScene(1);
    }

    private void DeleteSave()
    {
        string worldName = selectedWorldSaveSlot.worldSaveData.worldName;
        selectedWorldSaveSlot.RemoveSaveData();
        SaveSystem.RemoveSave(worldName);
    }

    private void OnSlotSelected(SaveSlotWidget slot)
    {
        selectedWorldSaveSlot = slot;
        OpenSaveSlotManagementMenu(selectedWorldSaveSlot);
    }

    private void OnSlotDeselected(SaveSlotWidget slot)
    {
        if (slot != selectedWorldSaveSlot) return;

        selectedWorldSaveSlot = null;
        CloseSaveSlotManagementMenu(slot);
    }

    private void OpenSaveSlotManagementMenu(SaveSlotWidget slot)
    {
        selectedWorldSaveSlot = slot;
        if (slot.worldSaveData != null) {
            OpenLoadWorldMenu();
        }
        else {
            CloseLoadWorldMenu();
            createWorldSlidePanel.OpenSlidePanel();
        }
    }

    private void CloseSaveSlotManagementMenu(SaveSlotWidget slot)
    {
        if (slot.worldSaveData != null) {
            CloseLoadWorldMenu();
        }
    }

    // Load World Menu
    private void OpenLoadWorldMenu()
    {
        loadSaveButton.gameObject.SetActive(true);
        deleteSaveButton.gameObject.SetActive(true);
    }

    private void CloseLoadWorldMenu()
    {
        loadSaveButton.gameObject.SetActive(false);
        deleteSaveButton.gameObject.SetActive(false);
    }

    // Create World Menu
    private void OnCreateWorldMenuOpen()
    {
        newWorldNameInputField.text = "";
        worldNameAlreadyExistsTextBlock.gameObject.SetActive(false);
        string name = newWorldNameInputField.text;
        CheckWorldName(name);

        selectedWorldSaveSlot.Button.IsInteractable = false;
    }

    private void OnCreateWorldMenuClose()
    {
        selectedWorldSaveSlot.Button.IsInteractable = true;
        selectedWorldSaveSlot.Button.OnRelease();
    }

    private void OnWorldNameInputFieldChangeValue(string value)
    {
        CheckWorldName(value);
    }

    private void CheckWorldName(string name)
    {
        if (!IsWorldNameFit(name)) {
            createWorldButton.SetState(CustomSelectableState.Disabled);
            return;
        }

        if (IsWorldNameExist(name)) {
            worldNameAlreadyExistsTextBlock.gameObject.SetActive(true);
            createWorldButton.SetState(CustomSelectableState.Disabled);
            return;
        }

        worldNameAlreadyExistsTextBlock.gameObject.SetActive(false);
        createWorldButton.SetState(CustomSelectableState.Idle);
    }

    private bool IsWorldNameExist(string name)
    {
        foreach (var data in SaveManager.Instance.allSaveData) {
            if (data != null && data.worldName == name) {
                return true;
            }
        }
        return false;
    }

    private bool IsWorldNameFit(string name)
    {
        if (name.Length > 0)
            return true;
        return
            false;
    }

    private void EnableSelectedSaveSlotInteractable()
    {
        //selectedWorldSaveSlot.Button.IsInteractable = true;
        //selectedWorldSaveSlot.Button.OnRelease();
    }

    private void DisableSelectedSaveSlotInteractable()
    {
        //selectedWorldSaveSlot.Button.IsInteractable = false;
    }
}
