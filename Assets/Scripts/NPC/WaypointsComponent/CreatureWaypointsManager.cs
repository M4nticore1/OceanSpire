using System.Collections.Generic;
using UnityEngine;

public class CreatureWaypointsManager : MonoBehaviour
{
    public static CreatureWaypointsManager Instance { get; private set; }

    private List<CreatureWaypointsComponent> waypointsComponents = new();

    private void Awake()
    {
        if (Instance) {
            Debug.LogError($"[{nameof(CreatureWaypointsManager)}] Another CraetureCityNavigatorsManager is alredy on the scene");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        foreach (var component in waypointsComponents) {
            if (component == null) {
                Debug.LogError($"[{nameof(CreatureWaypointsManager)}] Waypoint Component is not valid!");
                continue;
            }

            component.Tick();
        }
    }

    public void RegisterComponent(CreatureWaypointsComponent component)
    {
        if (component == null) return;
        if (waypointsComponents.Contains(component)) return;

        waypointsComponents.Add(component);
    }

    public void UnregisterComponent(CreatureWaypointsComponent component)
    {
        if (component == null) return;
        if (!waypointsComponents.Contains(component)) return;

        waypointsComponents.Remove(component);
    }
}