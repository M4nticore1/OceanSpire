using UnityEngine;

public class PlayerControllerLoader : WorldLoader
{
    [SerializeField] private PlayerController playerController;

    protected override void Load(WorldData worldData)
    {
        var playerControllerData = worldData?.Player;

        if (playerControllerData != null) {
            playerController.Init(playerControllerData);
        }
        else {
            playerController.Init();
        }
    }
}