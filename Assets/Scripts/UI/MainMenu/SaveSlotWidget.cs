using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct WorldSaveData
{
    public string worldName;
    public int floorsCount;
    public int residentsCount;
    public string lastSaveData;
}

public class SaveSlotWidget : MonoBehaviour
{
    public SaveData worldSaveData { get; private set; } = null;
    [SerializeField] private int slotIndex = 0;
    [field: SerializeField] public CustomSelectable button { get; private set; }
    [SerializeField] private GameObject createWorldMenu;
    [SerializeField] private GameObject loadWorldMenu;
    [SerializeField] private TextMeshProUGUI worldNameText;
    [SerializeField] private TextMeshProUGUI floorsCountText;
    [SerializeField] private TextMeshProUGUI residentsCountText;
    [SerializeField] private TextMeshProUGUI lastSaveDataText;
    [SerializeField] private Image worldThumb;

    //[Header("Background")]
    //[SerializeField] private Image background;
    //[SerializeField] Color selectedColor;
    //[SerializeField] Color desellectedColor;
    private bool isSelected = false;

    public static event System.Action<SaveSlotWidget> OnSaveSlotClicked;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        button.onRelease += Click;
    }

    private void OnDisable()
    {
        button.onRelease -= Click;
    }

    private void Start()
    {
        if (SaveManager.Instance.allSaveData.Length > slotIndex) {
            Debug.Log(SaveManager.Instance.allSaveData.Length);
            SaveData data = SaveManager.Instance.allSaveData[slotIndex];
            if (data != null) {
                SetSaveData(data);
            }
        }
    }

    public void SetSaveData(SaveData data)
    {
        worldSaveData = data;

        createWorldMenu.SetActive(false);
        loadWorldMenu.SetActive(true);

        worldNameText.text = data.worldName;
        floorsCountText.text += $"\n{data.builtFloorsCount.ToString()}";
        residentsCountText.text += $"\n{data.residentsCount.ToString()}";
        //lastSaveDataText.text += $"\n{data.lastSaveData.ToString()}";
    }

    public void RemoveSaveData()
    {
        worldSaveData = null;
        createWorldMenu.SetActive(true);
        loadWorldMenu.SetActive(false);
    }

    private void Click()
    {
        OnSaveSlotClicked?.Invoke(this);
    }
}
