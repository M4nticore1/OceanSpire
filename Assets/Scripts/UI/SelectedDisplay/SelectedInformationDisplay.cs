using UnityEngine;

public class SelectedInformationDisplay : SelectedDisplay
{
    [Header("Information")]
    [SerializeField] private InformationMenu informationMenu;
    [SerializeField] private CustomButton button;

    private Building building;

    protected override void OnSubscribe()
    {
        base.OnSubscribe();

        button.OnReleased.AddListener(OnButtonClicked);
    }

    protected override void OnUnsubscribe()
    {
        base.OnUnsubscribe();

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