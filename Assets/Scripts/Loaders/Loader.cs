using UnityEngine;

public abstract class Loader : MonoBehaviour
{
    public bool IsLoaded { get; private set; } = false;

    private void Start()
    {
        WorldData data = WorldSaveManager.Instance.currentSaveWorldData;

        Load(data);
        IsLoaded = true;
    }

    protected abstract void Load(WorldData data);
}