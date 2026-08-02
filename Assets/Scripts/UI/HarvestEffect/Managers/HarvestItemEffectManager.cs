using System.Collections.Generic;
using UnityEngine;

public abstract class HarvestItemEffectManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private HarvestEffectWidget harvestResourceWidgetPrefab;
    public HarvestEffectWidget HarvestResourceWidgetPrefab => harvestResourceWidgetPrefab;

    [Header("Cooldown")]
    [SerializeField] private float spawnWidgetCooldown = 0.1f;
    private double lastSpawnWidgetTime;

    private List<HarvestEffectWidget> spawnedWidgets = new();

    protected virtual void Awake()
    {

    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            var widget = spawnedWidgets[i];
            if (!widget) {
                spawnedWidgets.RemoveAt(i);
                continue;
            }

            widget.Tick();
        }
    }

    protected virtual void Subscribe()
    {

    }

    protected virtual void Unsubscribe()
    {

    }

    public bool TryCreateWidget(ItemInstance item, Transform transform, Vector3 startPosition, Vector3 targetPosition)
    {
        if (!ShouldCreateWidget(item)) return false;

        CreateWidget(item, transform, startPosition, targetPosition);
        return true;
    }

    private void CreateWidget(ItemInstance item, Transform transform, Vector3 startPosition, Vector3 targetPosition)
    {
        var widget = HarvestItemWidgetFactory.CreateWidget(harvestResourceWidgetPrefab, transform, item, startPosition, targetPosition);
        if (!widget) return;

        spawnedWidgets.Add(widget);
        lastSpawnWidgetTime = Time.timeAsDouble;
    }

    private bool ShouldCreateWidget(ItemInstance item)
    {
        if (item == null) return false;

        var count = spawnedWidgets.Count;
        if (count >= 1) {
            var lastSpawned = spawnedWidgets[count - 1];
            if (lastSpawned && item.Definition == lastSpawned.Item.Definition && Time.timeAsDouble - lastSpawnWidgetTime <= spawnWidgetCooldown) {
                return false;
            }
        }

        return true;
    }
}