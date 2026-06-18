using System.Collections.Generic;
using UnityEngine;

public class CreatureWaypointsManager : MonoBehaviour
{
    public static CreatureWaypointsManager Instance { get; private set; }

    private List<CreatureWaypointsComponent> waypointsComponents = new();

    private void Awake()
    {
        if (Instance) {
            Debug.LogError("Another CraetureCityNavigatorsManager is alredy on the scene");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        foreach (var component in waypointsComponents) {
            component.Tick();
        }
    }

    public void RegisterComponent(CreatureWaypointsComponent component)
    {
        if (waypointsComponents.Contains(component)) return;

        waypointsComponents.Add(component);
    }

    public void UnregisterComponent(CreatureWaypointsComponent component)
    {
        if (!waypointsComponents.Contains(component)) return;

        waypointsComponents.Remove(component);
    }
}