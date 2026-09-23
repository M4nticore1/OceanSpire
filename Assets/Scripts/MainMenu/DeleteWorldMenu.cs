using UnityEngine;

public class DeleteWorldMenu : MonoBehaviour
{
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private SaveSlotWidget saveSlotWidget;

    [SerializeField] private CustomButton deleteButton;
    [SerializeField] private CustomButton cancelButton;

    [SerializeField] private FitSizeToContent slidePanelFitSize;
    [SerializeField] private FitSizeToContent descriptionTextFitSize;

    private WorldData worldData;

    private void OnEnable()
    {
        deleteButton.OnReleased.AddListener(OnDeleteButtonClicked);
        cancelButton.OnReleased.AddListener(OnCancelButtonClicked); 
    }

    private void OnDisable()
    {
        deleteButton.OnReleased.RemoveListener(OnDeleteButtonClicked);
        cancelButton.OnReleased.RemoveListener(OnCancelButtonClicked);
    }

    public void Show(WorldData worldData)
    {
        if (worldData == null) {
            Debug.LogError($"[{nameof(DeleteWorldMenu)}] WorldData is not valid!");
            return;
        }

        this.worldData = worldData;

        slidePanel.Show();
        saveSlotWidget.SetSaveData(worldData);

        descriptionTextFitSize.UpdateSize();
        slidePanelFitSize.UpdateSize();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    private void OnDeleteButtonClicked()
    {
        if (worldData == null) return;

        WorldSaveSystem.DeleteSaveByWorldName(worldData.WorldName);
        Hide();
    }

    private void OnCancelButtonClicked()
    {
        Hide();
    }
}