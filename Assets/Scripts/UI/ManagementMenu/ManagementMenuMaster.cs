using UnityEngine;

public class ManagementMenuMaster : MonoBehaviour
{
    [SerializeField] private ConstructionMenu constructionMenu;
    [SerializeField] private StorageMenu storageMenu;

    [SerializeField] private CustomButton openConstructionMenuButton;
    [SerializeField] private CustomButton openStorageMenuButton;

    [SerializeField] private CustomButton constructionListsMenuButton;
    [SerializeField] private CustomButton storageListsMenuButton;

    [SerializeField] private CustomButton closeManagementMenuButton;

    [SerializeField] private GameObject content;

    private void OnEnable()
    {
        openConstructionMenuButton.onReleased += OnConstructionMenuButtonReleased;
        openStorageMenuButton.onReleased += OnStorageMenuButtonReleased;
        closeManagementMenuButton.onReleased += Close;

        constructionListsMenuButton.onReleased += OnConstructionListsButtonReleased;
        storageListsMenuButton.onReleased += OnStorageListsButtonReleased;

        EventBus.onBuildingWidgetBuildClicked += OnBuildingWidgetBuildClicked;
    }

    private void OnDisable()
    {
        openConstructionMenuButton.onReleased -= OnConstructionMenuButtonReleased;
        openStorageMenuButton.onReleased -= OnStorageMenuButtonReleased;
        closeManagementMenuButton.onReleased -= Close;

        constructionListsMenuButton.onReleased -= OnConstructionListsButtonReleased;
        storageListsMenuButton.onReleased -= OnStorageListsButtonReleased;

        EventBus.onBuildingWidgetBuildClicked -= OnBuildingWidgetBuildClicked;
    }

    private void Start()
    {
        Close();
    }

    // Construction Menu
    private void OnConstructionMenuButtonReleased()
    {
        Open();
        OpenConstructionMenu();

        constructionMenu.ResetOpenedList();
        storageMenu.ResetOpenedList();

        constructionListsMenuButton.SetState(CustomButtonState.Selected);
        constructionListsMenuButton.FinishTransitionAnimation();
        storageListsMenuButton.FinishTransitionAnimation();
    }

    private void OnStorageMenuButtonReleased()
    {
        Open();
        OpenStorageMenu();

        constructionMenu.ResetOpenedList();
        storageMenu.ResetOpenedList();

        storageListsMenuButton.SetState(CustomButtonState.Selected);
        storageListsMenuButton.FinishTransitionAnimation();
        constructionListsMenuButton.FinishTransitionAnimation();

    }

    private void OnConstructionListsButtonReleased()
    {
        OpenConstructionMenu();
    }

    private void OnStorageListsButtonReleased()
    {
        OpenStorageMenu();
    }

    private void OpenConstructionMenu()
    {
        constructionMenu.Open();
        storageMenu.Close();
    }

    private void OpenStorageMenu()
    {
        constructionMenu.Close();
        storageMenu.Open();
    }

    private void Open()
    {
        content.SetActive(true);
        InputStateManager.instance.SetGameplayInputBlocked(true);
    }

    private void Close()
    {
        content.SetActive(false);
        InputStateManager.instance.SetGameplayInputBlocked(false);
    }

    // Events
    private void OnBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        Close();
    }
}
