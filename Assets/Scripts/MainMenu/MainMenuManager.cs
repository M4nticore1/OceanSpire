using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private CreateNewWorldMenu createNewWorldMenu = null;
    [SerializeField] private CustomButton loadSaveButton = null;
    [SerializeField] private CustomButton deleteSaveButton = null;

    private SaveSlotWidget lastSelectedSaveSlot;

    private void OnEnable()
    {
        loadSaveButton.OnReleased.AddListener(OnLoadWorldButtonClicked);
        deleteSaveButton.OnReleased.AddListener(OnDeleteWorldButtonClicked);

        SaveSlotWidget.OnSaveSlotReleased += OnSaveSlotReleased;
        SaveSlotWidget.OnSaveSlotDeselected += OnSaveSlotDeselected;
    }

    private void OnDisable()
    {
        loadSaveButton.OnReleased.RemoveListener(OnLoadWorldButtonClicked);
        deleteSaveButton.OnReleased.RemoveListener(OnDeleteWorldButtonClicked);

        SaveSlotWidget.OnSaveSlotReleased -= OnSaveSlotReleased;
        SaveSlotWidget.OnSaveSlotDeselected -= OnSaveSlotDeselected;
    }

    private void Start()
    {
        loadSaveButton.SetState(CustomButtonState.Disabled);
        deleteSaveButton.SetState(CustomButtonState.Disabled);
    }

    private void OnLoadWorldButtonClicked()
    {
        if (!lastSelectedSaveSlot) {
            Debug.LogError("Selected SaveSlotWidget not found");
            return;
        }

        var data = lastSelectedSaveSlot.WorldSaveData;
        if (data == null) {
            Debug.Log($"WorldSaveData not found at {SaveSlotWidget.Selected}");
            return;
        }

        WorldSaveHandler.Instance.SetWorldData(data);
        SceneManager.LoadScene(1);
    }

    private void OnDeleteWorldButtonClicked()
    {
        if (!lastSelectedSaveSlot) {
            Debug.LogError("Selected SaveSlotWidget not found");
            return;
        }

        var data = lastSelectedSaveSlot.WorldSaveData;
        if (data == null) {
            Debug.Log($"WorldSaveData not found at {SaveSlotWidget.Selected}");
            return;
        }

        string worldName = data.WorldName;
        WorldSaveSystem.RemoveSaveByWorldName(worldName);

        WorldSaveHandler.Instance.FindSavesData();
        lastSelectedSaveSlot.RemoveSaveData();
    }

    private void OnSaveSlotReleased(SaveSlotWidget saveSlotWidget)
    {
        lastSelectedSaveSlot = saveSlotWidget;

        if (saveSlotWidget.WorldSaveData != null) {
            loadSaveButton.SetState(CustomButtonState.Idle);
            deleteSaveButton.SetState(CustomButtonState.Idle);
        }
        else {
            createNewWorldMenu.Open();
        }
    }

    private void OnSaveSlotDeselected(SaveSlotWidget saveSlotWidget)
    {
        StartCoroutine(DisableButtonsCoroutine());
    }

    private IEnumerator DisableButtonsCoroutine()
    {
        yield return new WaitForEndOfFrame();

        if (SaveSlotWidget.Selected) yield break;

        loadSaveButton.SetState(CustomButtonState.Disabled);
        deleteSaveButton.SetState(CustomButtonState.Disabled);
    }
}