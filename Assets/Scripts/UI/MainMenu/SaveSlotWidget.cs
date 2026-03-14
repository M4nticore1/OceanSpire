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
    [SerializeField] private CreateNewWorldMenu createNewWorldMenu;

    public WorldData worldSaveData { get; private set; } = null;
    [SerializeField] private int slotIndex = 0;
    [SerializeField] private CustomButton button;
    public CustomButton Button => button;
    [SerializeField] private GameObject createWorldMenu;
    [SerializeField] private GameObject loadWorldMenu;
    [SerializeField] private TextMeshProUGUI worldNameText;
    [SerializeField] private TextMeshProUGUI floorsCountText;
    [SerializeField] private TextMeshProUGUI residentsCountText;
    [SerializeField] private TextMeshProUGUI lastSaveDataText;
    [SerializeField] private Image worldThumbImage;

    public static event System.Action<SaveSlotWidget> onSaveSlotSelected;
    public static event System.Action<SaveSlotWidget> OnSaveSlotDeselected;

    private void OnEnable()
    {
        button.onReleased += OnReleased;
        createNewWorldMenu.onClosed += OnCreateMenuClosed;
    }

    private void OnDisable()
    {
        button.onReleased -= OnReleased;
        createNewWorldMenu.onClosed -= OnCreateMenuClosed;
    }

    private void Start()
    {
        WorldData[] worldData = WorldSaveManager.Instance.allSaveData;
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

    public void SetSaveData(WorldData saveData)
    {
        worldSaveData = saveData;

        createWorldMenu.SetActive(false);
        loadWorldMenu.SetActive(true);

        worldNameText.text = saveData.cityData.cityName;
        floorsCountText.text += $"\n{saveData.cityData?.floorsCount.ToString()}";
        residentsCountText.text += $"\n{saveData.citizensData.Length.ToString()}";
        //lastSaveDataText.text += $"\n{data.lastSaveData.ToString()}";

        Texture2D thumb = WorldSaveSystem.GetSaveScreenshotByWorldName(saveData.cityData.cityName);
        if (thumb) {
            Sprite sprite = Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            worldThumbImage.sprite = sprite;
        }
        else
            Debug.LogWarning("Save thumb is not found!");
    }

    public void RemoveSaveData()
    {
        worldSaveData = null;
        createWorldMenu.SetActive(true);
        loadWorldMenu.SetActive(false);
    }

    private void OnReleased()
    {
        if (worldSaveData == null) {
            button.IsInteractable = false;
        }

        createNewWorldMenu.Open();

        onSaveSlotSelected?.Invoke(this);
    }

    private void OnCreateMenuClosed()
    {
        button.IsInteractable = true;
        button.SetState(CustomButtonState.Idle);
    }
}
