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
    [SerializeField] private CustomSelectable button;
    public CustomSelectable Button => button;
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

    public static event System.Action<SaveSlotWidget> OnSaveSlotSelected;
    public static event System.Action<SaveSlotWidget> OnSaveSlotDeselected;

    private void OnEnable()
    {
        button.onSelected += () => OnSaveSlotSelected?.Invoke(this);
        button.onDeselected += () => OnSaveSlotDeselected?.Invoke(this);
    }

    private void OnDisable()
    {
        button.onSelected -= () => OnSaveSlotSelected?.Invoke(this);
        button.onDeselected -= () => OnSaveSlotDeselected?.Invoke(this);
    }

    private void Start()
    {
        if (SaveManager.Instance.allSaveData.Length > slotIndex) {
            SaveData[] datas = SaveManager.Instance.allSaveData;
            SaveData data = datas.Length > slotIndex ? datas[slotIndex] : null;
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
}
