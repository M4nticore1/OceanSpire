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

    public WorldData WorldSaveData { get; private set; }

    [SerializeField] private CustomButton button;
    public CustomButton Button => button;

    [SerializeField] private GameObject createWorldMenu;
    [SerializeField] private GameObject loadWorldMenu;
    [SerializeField] private TextMeshProUGUI worldNameText;
    [SerializeField] private TextMeshProUGUI floorsCountText;
    [SerializeField] private TextMeshProUGUI residentsCountText;
    [SerializeField] private TextMeshProUGUI lastSaveDataText;
    [SerializeField] private Image worldThumbImage;

    public static event Action<SaveSlotWidget> OnWorldDataSeted;
    public static event Action<SaveSlotWidget> OnWorldDataRemoved;

    public static event Action<SaveSlotWidget> OnSaveSlotReleased;
    public static event Action<SaveSlotWidget> OnSaveSlotSelected;
    public static event Action<SaveSlotWidget> OnSaveSlotDeselected;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnReleased);
        button.OnSelected.AddListener(OnSelected);
        button.OnDeselected.AddListener(OnDeselected);
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnReleased);
        button.OnSelected.RemoveListener(OnSelected);
        button.OnDeselected.RemoveListener(OnDeselected);
    }

    public void SetSaveData(WorldData worldData)
    {
        if (worldData == null) {
            RemoveSaveData();
            return;
        }

        WorldSaveData = worldData;

        createWorldMenu.SetActive(false);
        loadWorldMenu.SetActive(true);

        worldNameText.SetText(worldData.WorldName);
        floorsCountText.SetText(worldData.FloorFrameBuildings.Count.ToString());

        if (worldData.Citizens != null) {
            residentsCountText.SetText(worldData.Citizens.Count.ToString());
        }

        var date = DateTimeOffset.FromUnixTimeSeconds(worldData.SaveTime).DateTime;
        lastSaveDataText.SetText(date.ToString());

        var thumb = WorldSaveSystem.GetSaveScreenshotByWorldName(worldData.WorldName);
        if (thumb) {
            var sprite = Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            worldThumbImage.sprite = sprite;
        }
        else {
            Debug.LogWarning("Save thumb is not found!");
        }

        OnWorldDataSeted?.Invoke(this);
    }

    public void RemoveSaveData()
    {
        if (WorldSaveData == null) return;

        WorldSaveData = null;

        button.SetState(CustomButtonState.Idle);
        createWorldMenu.SetActive(true);
        loadWorldMenu.SetActive(false);

        OnWorldDataRemoved?.Invoke(this);
    }

    private void OnReleased()
    {
        OnSaveSlotReleased?.Invoke(this);
    }

    private void OnSelected()
    {
        Selected = this;
        OnSaveSlotSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        if (Selected == this)
            Selected = null;

        OnSaveSlotDeselected?.Invoke(this);
    }
}