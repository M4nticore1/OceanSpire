using TMPro;
using UnityEngine;

public class FloorsCounterWidget : MonoBehaviour
{
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private TextMeshProUGUI countText;

    private void OnEnable()
    {
        FloorFrameModule.OnFloorModuleInited += OnFloorInited;
    }

    private void OnDisable()
    {
        FloorFrameModule.OnFloorModuleInited -= OnFloorInited;
    }

    private void Start()
    {
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        var currentFloorsCount = buildingsManager.BuiltFloors.Count;
        var maxFloorsCount = buildingsManager.MaxFloorsCount;
        var text = $"{currentFloorsCount}/{maxFloorsCount}";
        countText.SetText(text);
    }

    private void OnFloorInited(FloorFrameModule floorFrameModule)
    {
        UpdateCounter();
    }
}