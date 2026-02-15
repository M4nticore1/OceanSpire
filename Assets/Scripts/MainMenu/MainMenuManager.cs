using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private RectTransform managementSaveMenu;
    [SerializeField] private SlidePanel createWorldMenuSlidePanel = null;
    [SerializeField] private TMP_InputField nameWorldInputField = null;
    [SerializeField] private CustomButton createWorldButton = null;
    [SerializeField] private CustomButton loadSaveButton = null;
    [SerializeField] private CustomButton deleteSaveButton = null;
    [SerializeField] private TextMeshProUGUI worldNameAlreadyExistsTextBlock = null;
    [SerializeField] private SaveSlotWidget[] saveSlots = { };

    public SaveSlotWidget selectedWorldSaveSlot { get; private set; } = null;
    private WorldData SelectedSaveData => selectedWorldSaveSlot ? selectedWorldSaveSlot.worldSaveData : null;

    private void OnEnable()
    {
        SaveSlotWidget.OnSaveSlotSelected += OnSlotSelected;
        SaveSlotWidget.OnSaveSlotDeselected += OnSlotDeselected;
        createWorldMenuSlidePanel.onOpened += OnCreateWorldMenuOpened;
        createWorldMenuSlidePanel.onClosed += OnCreateWorldMenuClosed;
        createWorldButton.onReleased += OnCreateWorldButtonClicked;
        loadSaveButton.onReleased += OnLoadWorldButtonClicked;
        deleteSaveButton.onReleased += OnDeleteWorldButtonClicked;
        nameWorldInputField.onValueChanged.AddListener(OnWorldNameInputFieldChangeValue);
    }

    private void OnDisable()
    {
        SaveSlotWidget.OnSaveSlotSelected -= OnSlotSelected;
        SaveSlotWidget.OnSaveSlotDeselected -= OnSlotDeselected;
        createWorldMenuSlidePanel.onOpened -= OnCreateWorldMenuOpened;
        createWorldMenuSlidePanel.onClosed -= OnCreateWorldMenuClosed;
        createWorldButton.onReleased -= OnCreateWorldButtonClicked;
        loadSaveButton.onReleased -= OnLoadWorldButtonClicked;
        deleteSaveButton.onReleased -= OnDeleteWorldButtonClicked;
        nameWorldInputField.onValueChanged.RemoveListener(OnWorldNameInputFieldChangeValue);
    }

    private void Start()
    {
        managementSaveMenu.gameObject.SetActive(false);
        nameWorldInputField.onFocusSelectAll = false;
    }

    private void OnCreateWorldButtonClicked()
    {
        string worldName = nameWorldInputField.text;
        WorldSaveManager.Instance.CreateWorld(worldName);
    }

    private void OnLoadWorldButtonClicked()
    {
        WorldData data = SelectedSaveData;
        WorldSaveManager.Instance.LoadWorld(data);
    }

    private void OnDeleteWorldButtonClicked()
    {
        string worldName = selectedWorldSaveSlot.worldSaveData.cityData.cityName;
        WorldSaveSystem.RemoveSaveByWorldName(worldName);
        WorldSaveManager.Instance.FindSavesData();
        selectedWorldSaveSlot.Button.SetState(CustomSelectableState.Idle);
        selectedWorldSaveSlot.RemoveSaveData();
    }

    // On slot select / deselect
    private void OnSlotSelected(SaveSlotWidget slot)
    {
        selectedWorldSaveSlot = slot;
        OpenSaveSlotManagementMenu(selectedWorldSaveSlot);
    }

    private void OnSlotDeselected(SaveSlotWidget slot)
    {
        if (slot != selectedWorldSaveSlot) return;

        CloseSaveSlotManagementMenu(slot);
        //StartCoroutine(ResetSelectedSlotCoroutine());
    }

    // Open / close slot management menu
    private void OpenSaveSlotManagementMenu(SaveSlotWidget slot)
    {
        if (slot.worldSaveData != null) {
            managementSaveMenu.gameObject.SetActive(true);
            return;
        }
        
        if (slot.worldSaveData == null) {
            managementSaveMenu.gameObject.SetActive(false);
            createWorldMenuSlidePanel.OpenSlidePanel();
            return;
        }
    }

    private void CloseSaveSlotManagementMenu(SaveSlotWidget slot)
    {
        if (slot.worldSaveData != null) {
            managementSaveMenu.gameObject.SetActive(false);
            return;
        }

        if (slot.worldSaveData == null) {
            return;
        }
    }

    // Create World Menu
    private void OnCreateWorldMenuOpened()
    {
        nameWorldInputField.text = "";
        worldNameAlreadyExistsTextBlock.gameObject.SetActive(false);
        string name = nameWorldInputField.text;
        CheckWorldName(name);
    }

    private void OnCreateWorldMenuClosed()
    {
        foreach (SaveSlotWidget slot in saveSlots) {
            slot.Button.IsInteractable = true;
        }
        selectedWorldSaveSlot.Button.SetState(CustomSelectableState.Idle);
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
        WorldData[] worldData = WorldSaveManager.Instance.allSaveData;
        if (worldData == null) {
            Debug.LogError("worldData is not valid.");
            return false;
        }

        foreach (var data in worldData) {
            if (data != null && data.cityData.cityName == name) {
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
