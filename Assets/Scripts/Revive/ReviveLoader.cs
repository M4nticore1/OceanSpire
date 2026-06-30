using UnityEngine;

public class ReviveLoader : WorldLoader
{
    [SerializeField] ReviveManager reviveManager;

    protected override void Load(WorldData worldData)
    {
        if (!reviveManager) {
            Debug.LogError("reviveManager is not valid to load");
            return;
        }

        var reviveData = worldData?.ReviveSystem;

        if (reviveData != null) {
            reviveManager.Init(reviveData);
        }
        else {
            reviveManager.Init();
        }
    }
}