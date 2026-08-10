using UnityEngine;

public class SelectedBuildingInformationDisplay : SelectedDisplay
{
    [Header("Information")]
    [SerializeField] private BuildingInformationMenu buildingInformationMenu;
    [SerializeField] private CustomButton button;

    private IInformationable informationable;

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

        informationable = selectComponent.GetComponent<IInformationable>();
        if (informationable == null) return false;

        if (informationable.GetInformationName() == null) return false;
        if (informationable.GetInformationDescription() == null) return false;
        if (informationable.GetInformationImage() == null) return false;

        return true;
    }

    private void OnButtonClicked()
    {
        if (informationable == null) return;

        buildingInformationMenu.Show(informationable);
    }
}