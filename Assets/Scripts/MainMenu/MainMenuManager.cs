using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private RectTransform managementSaveMenu;
    [SerializeField] private CustomButton loadSaveButton = null;
    [SerializeField] private CustomButton deleteSaveButton = null;

    public SaveSlotWidget selectedWorldSaveSlot { get; private set; } = null;
    private WorldData SelectedSaveData => selectedWorldSaveSlot ? selectedWorldSaveSlot.worldSaveData : null;

    private void OnEnable()
    {
        loadSaveButton.onReleased += OnLoadWorldButtonClicked;
        deleteSaveButton.onReleased += OnDeleteWorldButtonClicked;
    }

    private void OnDisable()
    {
        loadSaveButton.onReleased -= OnLoadWorldButtonClicked;
        deleteSaveButton.onReleased -= OnDeleteWorldButtonClicked;
    }

    private void Start()
    {
        managementSaveMenu.gameObject.SetActive(false);
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
        selectedWorldSaveSlot.Button.SetState(CustomButtonState.Idle);
        selectedWorldSaveSlot.RemoveSaveData();
    }
}
