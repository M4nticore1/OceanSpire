using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private CustomButton loadSaveButton = null;
    [SerializeField] private CustomButton deleteSaveButton = null;

    private void OnEnable()
    {
        loadSaveButton.OnReleased.AddListener(OnLoadWorldButtonClicked);
        deleteSaveButton.OnReleased.AddListener(OnDeleteWorldButtonClicked);
        SaveSlotWidget.OnSaveSlotSelected += OnSaveSlotSelected;
        SaveSlotWidget.OnSaveSlotDeselected += OnSaveSlotDeselected;
    }

    private void OnDisable()
    {
        loadSaveButton.OnReleased.RemoveListener(OnLoadWorldButtonClicked);
        deleteSaveButton.OnReleased.RemoveListener(OnDeleteWorldButtonClicked);
        SaveSlotWidget.OnSaveSlotSelected -= OnSaveSlotSelected;
        SaveSlotWidget.OnSaveSlotDeselected -= OnSaveSlotDeselected;
    }

    private void Start()
    {
        loadSaveButton.SetState(CustomButtonState.Disabled);
        deleteSaveButton.SetState(CustomButtonState.Disabled);
    }

    private void OnLoadWorldButtonClicked()
    {
        var selectedSaveSlot = SaveSlotWidget.Selected;
        if (!selectedSaveSlot) {
            Debug.LogError("Selected SaveSlotWidget not found");
            return;
        }

        var data = selectedSaveSlot.WorldSaveData;
        if (data == null) {
            Debug.Log($"WorldSaveData not found at {SaveSlotWidget.Selected}");
            return;
        }

        WorldSaveManager.Instance.SetWorldData(data);
        SceneManager.LoadScene(1);
    }

    private void OnDeleteWorldButtonClicked()
    {
        var selectedSaveSlot = SaveSlotWidget.Selected;
        if (!selectedSaveSlot) {
            Debug.LogError("Selected SaveSlotWidget not found");
            return;
        }

        var data = selectedSaveSlot.WorldSaveData;
        if (data == null) {
            Debug.Log($"WorldSaveData not found at {SaveSlotWidget.Selected}");
            return;
        }

        string worldName = data.WorldName;
        WorldSaveSystem.RemoveSaveByWorldName(worldName);

        WorldSaveManager.Instance.FindSavesData();
        SaveSlotWidget.Selected.RemoveSaveData();
    }

    private void OnSaveSlotSelected(SaveSlotWidget saveSlotWidget)
    {
        loadSaveButton.SetState(CustomButtonState.Idle);
        deleteSaveButton.SetState(CustomButtonState.Idle);
    }

    private void OnSaveSlotDeselected(SaveSlotWidget saveSlotWidget)
    {
        loadSaveButton.SetState(CustomButtonState.Disabled);
        deleteSaveButton.SetState(CustomButtonState.Disabled);
    }
}