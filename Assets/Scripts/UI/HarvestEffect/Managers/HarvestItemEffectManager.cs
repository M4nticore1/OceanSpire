using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HarvestItemEffectManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private HarvestEffectWidget harvestResourceWidgetPrefab;
    public HarvestEffectWidget HarvestResourceWidgetPrefab => harvestResourceWidgetPrefab;

    [Header("Cooldown")]
    [SerializeField] private float spawnWidgetCooldown = 0.1f;
    private Dictionary<ItemDefinition, double> lastSpawnTimes = new();

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
            if (widget == null) {
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

    public bool TryCreateWidget(HarvestEffectWidget widget, Transform transform, Vector3 startPosition, Vector3 targetPosition, ItemInstance item, bool useLocalTransform)
    {
        if (!ShouldCreateWidget(item)) return false;

        CreateWidget(widget, transform, startPosition, targetPosition, item, useLocalTransform);
        lastSpawnTimes[item.Definition] = Time.timeAsDouble;

        return true;
    }

    public IEnumerator CreateWidgetsCoroutine(HarvestEffectWidget widget, Transform transform, Vector3 startPosition, Vector3 targetPosition, List<ItemInstance> items, bool useLocalTransform)
    {
        var wait = new WaitForSeconds(spawnWidgetCooldown);

        for (int i = 0; i < items.Count; i++) {
            var item = items[i];
            if (item == null) continue;

            TryCreateWidget(widget, transform, startPosition, targetPosition, item, useLocalTransform);
            yield return wait;
        }
    }

    private void CreateWidget(HarvestEffectWidget widget, Transform transform, Vector3 startPosition, Vector3 targetPosition, ItemInstance item, bool useLocalTransform)
    {
        var spawnedWidget = HarvestItemWidgetFactory.CreateWidget(widget, transform, item, startPosition, targetPosition, useLocalTransform);
        if (spawnedWidget != null) {
            spawnedWidgets.Add(spawnedWidget);
        }
    }

    private bool ShouldCreateWidget(ItemInstance item)
    {
        if (item == null) return false;

        if (lastSpawnTimes.TryGetValue(item.Definition, out double lastTime)) {
            if (Time.timeAsDouble - lastTime < spawnWidgetCooldown) {
                return false;
            }
        }

        return true;
    }
}