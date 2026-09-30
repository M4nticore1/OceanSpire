using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateWorldMenu : ManageWorldMenu
{
    private WorldSavesHandler worldSavesHandler => WorldSavesHandler.Instance;

    protected override void HandleShown()
    {
        base.HandleShown();

        InputField.text = "";
    }

    protected override void HandleActionButtonClicked()
    {
        base.HandleActionButtonClicked();

        if (worldSavesHandler == null) {
            Debug.LogError($"[{nameof(CreateWorldMenu)}] WorldSavesHandler is not valid!");
            return;
        }

        var worldName = InputField.text;

        var worldData = WorldData.Default();
        worldData.WorldName = worldName;

        worldSavesHandler.SetWorldData(worldData);
        SceneManager.LoadScene(1);
    }
}