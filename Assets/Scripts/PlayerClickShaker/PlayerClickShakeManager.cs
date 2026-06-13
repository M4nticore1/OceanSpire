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
            shaker.Tick();
        }
    }

    public void RegisterShaker(PlayerClickShaker shaker)
    {
        shakers.Add(shaker);
    }

    public void UnregisterShaker(PlayerClickShaker shaker)
    {
        shakers.Remove(shaker);
    }
}