using UnityEngine;

public abstract class Loader : MonoBehaviour
{
    public bool isLoaded { get; private set; } = false;

    private void Start()
    {
        WorldData data = WorldSaveManager.Instance.currentSaveWorldData;

        Load(data);
        isLoaded = true;
    }

    protected abstract void Load(WorldData data);
}