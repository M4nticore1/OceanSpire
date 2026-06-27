using UnityEngine;

public abstract class PlayerLoader : MonoBehaviour
{
    public bool IsLoaded { get; private set; } = false;

    private void Start()
    {
        var playerData = PlayerSaveSystem.GetData();

        if (playerData == null) {
            playerData = PlayerData.Default();
        }

        Load(playerData);
        IsLoaded = true;
    }

    protected abstract void Load(PlayerData playerData);
}