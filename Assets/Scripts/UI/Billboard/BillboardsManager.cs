using System.Collections.Generic;
using UnityEngine;

public class BillboardsManager : MonoBehaviour
{
    public static BillboardsManager Instance { get; private set; }

    private readonly HashSet<Billboard> billboards = new();

    private void Awake()
    {
        if (Instance) {
            Debug.LogError($"[{nameof(BillboardsManager)}] Another instance already exists in the scene! Destroying this.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(Billboard billboard)
    {
        if (!billboard) return;

        billboards.Add(billboard);
    }

    public void Unregister(Billboard billboard)
    {
        if (!billboard) return;

        billboards.Remove(billboard);
    }

    private void LateUpdate()
    {
        foreach (var billboard in billboards) {
            billboard.Tick();
        }
    }
}