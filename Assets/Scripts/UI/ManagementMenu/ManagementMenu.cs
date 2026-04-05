using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class ManagementMenu : MonoBehaviour
{
    [SerializeField] protected GridLayoutGroup[] lists;
    [SerializeField] private CustomButton[] listButtons;
    [SerializeField] private int[] includedCategoies;
    [SerializeField] protected ScrollRect listsScrollRect;

    protected int lastOpenedBuildingsListCategory = 0;
    protected Action[] buttonCallbacks;

    private void Awake()
    {
        buttonCallbacks = new Action[includedCategoies.Length];

        foreach (GridLayoutGroup rect in lists) {
            rect.gameObject.SetActive(false);
        }
    }

    protected virtual void Start()
    {
        CreateWidgets();
    }

    private void OnEnable()
    {
        for (int i = 0; i < lists.Length; i++) {
            int index = i;
            buttonCallbacks[index] = () => OnListButtonClicked(index);
            listButtons[index].onReleased += buttonCallbacks[index];
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < lists.Length; i++) {
            int index = i;
            listButtons[index].onReleased -= buttonCallbacks[index];
        }
    }

    protected abstract void CreateWidgets();

    public void ResetOpenedList()
    {
        CustomButton lastButton = listButtons[lastOpenedBuildingsListCategory];

        CloseListByCategory(lastOpenedBuildingsListCategory);
        lastOpenedBuildingsListCategory = 0;

        OpenListByCategory(lastOpenedBuildingsListCategory);

        CustomButton newButton = listButtons[lastOpenedBuildingsListCategory];
        newButton.SetState(CustomButtonState.Selected);

        lastButton.FinishTransitionAnimation();
        newButton.FinishTransitionAnimation();
    }

    private void OpenListByCategory(int index)
    {
        lists[index].gameObject.SetActive(true);
        listButtons[index].transform.SetAsLastSibling();

        listsScrollRect.content = lists[index].GetComponent<RectTransform>();
        lastOpenedBuildingsListCategory = index;
    }

    private void CloseListByCategory(int index)
    {
        listButtons[index].transform.SetSiblingIndex(listButtons.Length - index - 1);
        lists[index].gameObject.SetActive(false);
    }

    // Events
    private void OnListButtonClicked(int index)
    {
        CloseListByCategory(lastOpenedBuildingsListCategory);
        OpenListByCategory(index);
    }

    // Toggle
    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
