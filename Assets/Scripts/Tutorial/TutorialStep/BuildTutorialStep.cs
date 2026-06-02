using UnityEngine;

public class BuildTutorialStep : TutorialStep
{
    [SerializeField] private ConstructionsManagementList constructionMenu;

    protected override void OnShow()
    {
        base.OnShow();

        constructionMenu.GetBuildingWidget((int)BuildingIdEnum.CoalGenerator).BuildButton.enabled = true;
    }
}