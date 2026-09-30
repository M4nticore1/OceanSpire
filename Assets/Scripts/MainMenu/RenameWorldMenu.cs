using UnityEngine;

public class RenameWorldMenu : ManageWorldMenu
{
    public static RenameWorldMenu Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null) {
            Instance = this;
        }
        else {
            Debug.LogError($"[{nameof(RenameWorldMenu)}] RenameWorldMenu is not valid!");
            Destroy(gameObject);
        }
    }

    protected override void HandleShown()
    {
        base.HandleShown();

        InputField.text = worldData.WorldName;
    }

    protected override void HandleActionButtonClicked()
    {
        base.HandleActionButtonClicked();

        if (worldData != null) {
            WorldSaveSystem.RenameWorld(worldData.WorldName, InputField.text);
        }
        else {
            Debug.LogError($"[{nameof(RenameWorldMenu)}] WorldData is not valid!");
        }
    }
}