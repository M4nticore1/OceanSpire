using System.Collections.Generic;
using UnityEngine;

public class PlayerClickShakeManager : MonoBehaviour
{
    public static PlayerClickShakeManager Instance { get; private set; }

    private List<PlayerClickShaker> shakers = new();

    private void Awake()
    {
        if (Instance) {
            Debug.LogError("Another Player Click Shake Manager is already on the scene");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        foreach (var shaker in shakers) {
            if (!shaker) {
                Debug.LogError($"[{nameof(CreatureWaypointsManager)}] Shaker is not valid!");
                continue;
            }

            shaker.Tick();
        }
    }

    public void RegisterShaker(PlayerClickShaker shaker)
    {
        if (shakers.Contains(shaker)) return;

        shakers.Add(shaker);
    }

    public void UnregisterShaker(PlayerClickShaker shaker)
    {
        if (!shaker) return;
        if (!shakers.Contains(shaker)) return;

        shakers.Remove(shaker);
    }
}