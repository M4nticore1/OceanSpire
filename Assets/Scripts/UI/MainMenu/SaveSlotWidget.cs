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

    [SerializeField] private CreateNewWorldMenu createNewWorldMenu;

    public WorldData WorldSaveData { get; private set; } = null;
    [SerializeField] private int slotIndex = 0;

    [SerializeField] private CustomButton button;
    [SerializeField] private GameObject createWorldMenu;
    [SerializeField] private GameObject loadWorldMenu;
    [SerializeField] private TextMeshProUGUI worldNameText;
    [SerializeField] private TextMeshProUGUI floorsCountText;
    [SerializeField] private TextMeshProUGUI residentsCountText;
    [SerializeField] private TextMeshProUGUI lastSaveDataText;
    [SerializeField] private Image worldThumbImage;

    public static event System.Action<SaveSlotWidget> OnSaveSlotSelected;
    public static event System.Action<SaveSlotWidget> OnSaveSlotDeselected;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnClicked);
        button.OnSelected.AddListener(OnSelected);
        button.OnDeselected.AddListener(OnDeselected);
        createNewWorldMenu.onClosed += OnCreateMenuClosed;
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnClicked);
        button.OnSelected.RemoveListener(OnSelected);
        button.OnDeselected.RemoveListener(OnDeselected);
        createNewWorldMenu.onClosed -= OnCreateMenuClosed;
    }

    private void Start()
    {
        var worldData = WorldSaveManager.Instance.AllSaveData;
        if (worldData == null) return;

        if (worldData.Length > slotIndex) {
            if (worldData.Length <= slotIndex) {
                Debug.Log("A length of worldData array is less than slot index.");
                return;
            }

            WorldData data = worldData[slotIndex];
            if (data != null) {
                SetSaveData(data);
            }
        }
    }

    public void SetSaveData(WorldData worldData)
    {
        WorldSaveData = worldData;

        createWorldMenu.SetActive(false);
        loadWorldMenu.SetActive(true);

        worldNameText.SetText(worldData.WorldName);
        floorsCountText.SetText(worldData.FloorFrameBuildings.Length.ToString());

        if (worldData.Citizens != null) {
            residentsCountText.SetText(worldData.Citizens.Length.ToString());
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
    }

    public void RemoveSaveData()
    {
        WorldSaveData = null;

        button.SetState(CustomButtonState.Idle);
        createWorldMenu.SetActive(true);
        loadWorldMenu.SetActive(false);
    }

    private void OnClicked()
    {
        Selected = this;

        if (WorldSaveData == null) {
            createNewWorldMenu.Open();
            button.SetInteractable(false);
        }
    }

    private void OnSelected()
    {
        OnSaveSlotSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        OnSaveSlotDeselected?.Invoke(this);
    }

    private void OnCreateMenuClosed()
    {
        Selected = null;
        button.SetInteractable(true);
        button.SetState(CustomButtonState.Idle);
    }
}
