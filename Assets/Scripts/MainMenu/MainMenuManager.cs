using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private CreateWorldMenu createNewWorldMenu;
    [SerializeField] private DeleteWorldMenu deleteWorldMenu;

    [SerializeField] private CustomButton loadSaveButton;
    [SerializeField] private CustomButton deleteSaveButton;

    private SaveSlotWidget lastSelectedSaveSlot;

    private void OnEnable()
    {
        loadSaveButton.OnReleased.AddListener(HandleLoadWorldButtonClicked);
        deleteSaveButton.OnReleased.AddListener(HandleDeleteWorldButtonClicked);

        SaveSlotWidget.OnSaveSlotSelected += HandleSaveSlotSelected;
        SaveSlotWidget.OnSaveSlotDeselected += HandleSaveSlotDeselected;
    }

    private void OnDisable()
    {
        loadSaveButton.OnReleased.RemoveListener(HandleLoadWorldButtonClicked);
        deleteSaveButton.OnReleased.RemoveListener(HandleDeleteWorldButtonClicked);

        SaveSlotWidget.OnSaveSlotSelected -= HandleSaveSlotSelected;
        SaveSlotWidget.OnSaveSlotDeselected -= HandleSaveSlotDeselected;
    }

    private void Start()
    {
        loadSaveButton.SetState(CustomButtonState.Disabled);
        deleteSaveButton.SetState(CustomButtonState.Disabled);
    }

    private void HandleLoadWorldButtonClicked()
    {
        if (!lastSelectedSaveSlot) {
            Debug.LogError($"[{nameof(MainMenuManager)}] Selected SaveSlotWidget not found!");
            return;
        }

        var data = lastSelectedSaveSlot.WorldSaveData;
        if (data == null) {
            Debug.Log($"[{nameof(MainMenuManager)}] WorldSaveData not found at {SaveSlotWidget.Selected}!");
            return;
        }

        WorldSaveHandler.Instance.SetWorldData(data);
        SceneManager.LoadScene(1);
    }

    private void HandleDeleteWorldButtonClicked()
    {
        if (lastSelectedSaveSlot == null) {
            Debug.LogError($"[{nameof(MainMenuManager)}] Selected SaveSlotWidget not found!");
            return;
        }

        var data = lastSelectedSaveSlot.WorldSaveData;
        if (data == null) {
            Debug.Log($"[{nameof(MainMenuManager)}] WorldSaveData not found at {SaveSlotWidget.Selected}!");
            return;
        }

        deleteWorldMenu.Show(data);
    }

    private void HandleSaveSlotSelected(SaveSlotWidget saveSlotWidget)
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

    private void HandleSaveSlotDeselected(SaveSlotWidget saveSlotWidget)
    {
        StartCoroutine(HandleSaveSlotDeselectedEndOfFrame());
    }

    private IEnumerator HandleSaveSlotDeselectedEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (SaveSlotWidget.Selected != null) yield break;

        loadSaveButton.SetState(CustomButtonState.Disabled);
        deleteSaveButton.SetState(CustomButtonState.Disabled);
    }
}