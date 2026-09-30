using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldEntry
{
    public string worldName;
    public int floorsCount;
    public int residentsCount;
    public string lastSaveData;
}

public class SaveSlotWidget : MonoBehaviour
{
    public static SaveSlotWidget Selected { get; private set; }

    [Header("UI")]
    [SerializeField] private CustomButton button;
    public CustomButton Button => button;

    [SerializeField] private CustomButton renameButton;
    public CustomButton RenameButton => renameButton;

    [Header("Panels")]
    [SerializeField] private GameObject createWorldMenu;
    [SerializeField] private GameObject loadWorldMenu;

    [Header("Save Data")]
    [SerializeField] private TextMeshProUGUI worldNameText;
    [SerializeField] private TextMeshProUGUI floorsCountText;
    [SerializeField] private TextMeshProUGUI residentsCountText;
    [SerializeField] private TextMeshProUGUI lastSaveDataText;
    [SerializeField] private Image worldThumbImage;

    public WorldData WorldData { get; private set; }

    private RenameWorldMenu renameWorldMenu => RenameWorldMenu.Instance;

    public static event Action<SaveSlotWidget> OnWorldDataSet;
    public static event Action<SaveSlotWidget> OnWorldDataRemoved;

    public static event Action<SaveSlotWidget> OnSaveSlotReleased;
    public static event Action<SaveSlotWidget> OnSaveSlotSelected;
    public static event Action<SaveSlotWidget> OnSaveSlotDeselected;

    private void OnEnable()
    {
        WorldSaveSystem.OnWorldSaveDeleted += HandleWorldDataDeleted;

        if (button != null) {
            button.OnReleased.AddListener(HandleButtonClicked);
            button.OnSelected.AddListener(HandleButtonSelected);
            button.OnDeselected.AddListener(HandleDeselected);
        }

        if (renameButton != null) {
            renameButton.OnReleased.AddListener(HandleRenameButtonClicked);
        }
    }

    private void OnDisable()
    {
        WorldSaveSystem.OnWorldSaveDeleted -= HandleWorldDataDeleted;

        if (button != null) {
            button.OnReleased.RemoveListener(HandleButtonClicked);
            button.OnSelected.RemoveListener(HandleButtonSelected);
            button.OnDeselected.RemoveListener(HandleDeselected);
        }

        if (renameButton != null) {
            renameButton.OnReleased.RemoveListener(HandleRenameButtonClicked);
        }
    }

    private void Start()
    {
        UpdatePanelsActive();
    }

    public void SetSaveData(WorldData worldData)
    {
        if (worldData == null) {
            RemoveSaveData();
            return;
        }

        WorldData = worldData;

        worldNameText.SetText(worldData.WorldName);
        floorsCountText.SetText(worldData.FloorFrameBuildings.Count.ToString());

        if (worldData.Citizens != null) {
            residentsCountText.SetText(worldData.Citizens.Count.ToString());
        }

        var date = DateTimeOffset.FromUnixTimeSeconds(worldData.SaveTime).DateTime;
        lastSaveDataText.SetText(date.ToString());

        var thumb = WorldSaveSystem.GetSaveScreenshotByWorldName(worldData.WorldName);
        if (thumb != null) {
            var sprite = Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            worldThumbImage.sprite = sprite;
        }

        UpdatePanelsActive();

        OnWorldDataSet?.Invoke(this);
    }

    public void RemoveSaveData()
    {
        if (WorldData == null) return;

        WorldData = null;

        button.SetState(CustomButtonState.Idle);

        UpdatePanelsActive();

        OnWorldDataRemoved?.Invoke(this);
    }

    private void UpdatePanelsActive()
    {
        if (WorldData != null) {
            createWorldMenu.SetActive(false);
            loadWorldMenu.SetActive(true);
        }
        else {
            createWorldMenu.SetActive(true);
            loadWorldMenu.SetActive(false);
        }
    }

    private void HandleButtonClicked()
    {
        OnSaveSlotReleased?.Invoke(this);
    }

    private void HandleButtonSelected()
    {
        Selected = this;
        OnSaveSlotSelected?.Invoke(this);
    }

    private void HandleDeselected()
    {
        if (Selected == this)
            Selected = null;

        OnSaveSlotDeselected?.Invoke(this);
    }

    private void HandleRenameButtonClicked()
    {
        if (renameWorldMenu == null) return;

        renameWorldMenu.Show(WorldData);
    }

    private void HandleWorldDataDeleted(WorldData worldData)
    {
        if (worldData == null) return;
        if (worldData != WorldData) return;

        RemoveSaveData();
    }
}