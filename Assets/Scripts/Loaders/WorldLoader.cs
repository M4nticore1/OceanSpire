using UnityEngine;

public abstract class WorldLoader : MonoBehaviour
{
    public bool IsLoaded { get; private set; } = false;

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        var data = WorldSavesHandler.Instance.CurrentWorldData;

        Load(data);
        IsLoaded = true;
    }

    protected abstract void Load(WorldData worldData);
}