using UnityEngine;

public abstract class WorldLoader : MonoBehaviour
{
    public bool IsLoaded { get; private set; } = false;

    private void Start()
    {
        var data = WorldSaveHandler.Instance.CurrentWorldData;

        Load(data);
        IsLoaded = true;
    }

    protected abstract void Load(WorldData worldData);
}