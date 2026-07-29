using UnityEngine;

public class BuildingInformationMenu : InformationMenu
{
    public static BuildingInformationMenu Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance) {
            Debug.LogError($"[{nameof(BuildingInformationMenu)}] There is another Building Information Menu in the scene!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}