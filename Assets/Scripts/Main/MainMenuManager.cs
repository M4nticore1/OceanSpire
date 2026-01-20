using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField newWorldNameInputField = null;
    [SerializeField] private Button createWorldButton = null;
    [SerializeField] private Button loadSaveButton = null;
    [SerializeField] private Button deleteSaveButton = null;
    [SerializeField] private TextMeshProUGUI worldNameAlreadyExistsTextBlock = null;

    public SaveSlotWidget selectedWorldSaveSlot;
    [SerializeField] private SlidePanel createWorldSlidePanel = null;

    private void Awake()
    {
        createWorldButton.onClick.AddListener(CreateWorld);
        loadSaveButton.onClick.AddListener(LoadSave);
        deleteSaveButton.onClick.AddListener(DeleteSave);
        newWorldNameInputField.onValueChanged.AddListener(OnWorldNameInputFieldChangeValue);
    }

    private void OnEnable()
    {
        SaveSlotWidget.OnSaveSlotClicked += OnSlotClick;
    }

    private void OnDisable()
    {
        SaveSlotWidget.OnSaveSlotClicked -= OnSlotClick;
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

    private void OnSlotClick(SaveSlotWidget slot)
    {
        OpenSaveSlotManagementMenu(slot);
    }

    private void OpenSaveSlotManagementMenu(SaveSlotWidget slot)
    {
        Debug.Log("OnSlotClick");
        selectedWorldSaveSlot = slot;
        if (slot.worldSaveData != null) {
            OpenLoadWorldMenu();
        }
        else {
            CloseLoadWorldMenu();
            OpenCreateWorldMenu();
        }
    }

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

    private void OpenCreateWorldMenu()
    {
        createWorldSlidePanel.OpenSlidePanel();
        newWorldNameInputField.text = "";
        worldNameAlreadyExistsTextBlock.gameObject.SetActive(false);
        string name = newWorldNameInputField.text;
        CheckWorldName(name);
    }

    private void OnWorldNameInputFieldChangeValue(string value)
    {
        CheckWorldName(value);
    }

    private void CheckWorldName(string name)
    {
        if (!IsWorldNameFit(name)) {
            createWorldButton.interactable = false;
            return;
        }

        if (IsWorldNameExist(name)) {
            worldNameAlreadyExistsTextBlock.gameObject.SetActive(true);
            createWorldButton.interactable = false;
            return;
        }

        worldNameAlreadyExistsTextBlock.gameObject.SetActive(false);
        createWorldButton.interactable = true;
    }

    private bool IsWorldNameExist(string name)
    {
        foreach (var data in SaveManager.Instance.allSaveData) {
            if (data.worldName == name) {
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
}
