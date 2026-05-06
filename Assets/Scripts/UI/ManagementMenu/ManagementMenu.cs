using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ManagementMenu : UIBehaviour
{
    [SerializeField] protected GameObject managementMenu;
    [SerializeField] protected CustomButton openListButton;
    [SerializeField] protected ManagementList[] lists;
    [SerializeField] private CustomButton[] listButtons;

    protected int lastOpenedBuildingsListCategory = 0;

    protected override void Start()
    {
        base.Start();

        CreateWidgets();
    }

    protected abstract void CreateWidgets();

    public void Open()
    {
        openListButton.SetState(CustomButtonState.Selected);

        managementMenu.SetActive(true);
        gameObject.SetActive(true);

        ResetScrollRects();
    }

    public void Close()
    {
        managementMenu.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ResetOpenedList()
    {
        CustomButton lastButton = listButtons[lastOpenedBuildingsListCategory];

        CloseListByCategory(lastOpenedBuildingsListCategory);
        lastOpenedBuildingsListCategory = 0;

        OpenListByCategory(lastOpenedBuildingsListCategory);

        CustomButton newButton = listButtons[lastOpenedBuildingsListCategory];
        newButton.SetState(CustomButtonState.Selected);

        lastButton.EndTransitionAnimation();
        newButton.EndTransitionAnimation();
    }

    public void OpenListByCategory(int index)
    {
        CloseAllLists();

        lists[index].gameObject.SetActive(true);
        listButtons[index].SetState(CustomButtonState.Selected);

        for (int i = 0; i < listButtons.Length; i++) {
            listButtons[i].transform.SetAsFirstSibling();
        }

        listButtons[index].transform.SetAsLastSibling();

        //scrollRect.content = lists[index].GetComponent<RectTransform>();
        lastOpenedBuildingsListCategory = index;
    }

    public void OpenLastOpenedList()
    {
        OpenListByCategory(lastOpenedBuildingsListCategory);
    }

    public void CloseListByCategory(int index)
    {
        lists[index].gameObject.SetActive(false);
    }

    public void CloseLastOpenedList()
    {
        CloseListByCategory(lastOpenedBuildingsListCategory);
    }

    private void CloseAllLists()
    {
        for (int i = 0; i < lists.Length; i++) {
            CloseListByCategory(i);
        }
    }

    private void ResetScrollRects()
    {
        foreach (var list in lists) {
            list.ScrollRect.verticalNormalizedPosition = 1f;
        }
    }
}