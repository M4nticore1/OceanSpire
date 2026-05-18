using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject managementSaveMenu;
    [SerializeField] private CustomButton loadSaveButton = null;
    [SerializeField] private CustomButton deleteSaveButton = null;

    private void OnEnable()
    {
        loadSaveButton.OnReleased.AddListener(OnLoadWorldButtonClicked);
        deleteSaveButton.OnReleased.AddListener(OnDeleteWorldButtonClicked);
        SaveSlotWidget.onSaveSlotSelected += OnSaveSlotSelected;
        SaveSlotWidget.onSaveSlotDeselected += OnSaveSlotDeselected;
    }

    private void OnDisable()
    {
        loadSaveButton.OnReleased.RemoveListener(OnLoadWorldButtonClicked);
        deleteSaveButton.OnReleased.RemoveListener(OnDeleteWorldButtonClicked);
        SaveSlotWidget.onSaveSlotSelected -= OnSaveSlotSelected;
        SaveSlotWidget.onSaveSlotDeselected -= OnSaveSlotDeselected;
    }

    private void Start()
    {
        managementSaveMenu.gameObject.SetActive(false);
    }

    private void OnLoadWorldButtonClicked()
    {
        WorldData data = SaveSlotWidget.Selected.WorldSaveData;

        WorldSaveManager.Instance.SetWorldData(data);
        SceneManager.LoadScene(1);
    }

    private void OnDeleteWorldButtonClicked()
    {
        string worldName = SaveSlotWidget.Selected.WorldSaveData.WorldName;
        WorldSaveSystem.RemoveSaveByWorldName(worldName);

        WorldSaveManager.Instance.FindSavesData();
        SaveSlotWidget.Selected.RemoveSaveData();
    }

    private void OnSaveSlotSelected(SaveSlotWidget saveSlotWidget)
    {
        managementSaveMenu.SetActive(true);
    }

    private void OnSaveSlotDeselected(SaveSlotWidget saveSlotWidget)
    {
        managementSaveMenu.SetActive(false);
    }
}
