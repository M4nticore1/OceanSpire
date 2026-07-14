using UnityEngine;

public class SelectedInformationDisplay : SelectedDisplay
{
    [Header("Information")]
    [SerializeField] private InformationMenu informationMenu;
    [SerializeField] private CustomButton button;

    private Building building;

    protected override void Subscribe()
    {
        base.Subscribe();

        button.OnReleased.AddListener(OnButtonClicked);
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        button.OnReleased.RemoveListener(OnButtonClicked);
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        building = selectComponent.GetComponent<Building>();
        if (!building) return false;

        return true;
    }

    private void OnButtonClicked()
    {
        if (!building) return;

        informationMenu.Show(building);
    }
}