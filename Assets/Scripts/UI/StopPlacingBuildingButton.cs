using UnityEngine;

public class StopPlacingBuildingButton : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private CustomButton button;

    private void Start()
    {
        Hide();
    }

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnButtonClicked);
        EventBus.OnConstructionStarted += OnStartedPlacingBuilding;
        Building.OnBuildingInited += OnBuildingInited;
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);
        EventBus.OnConstructionStarted -= OnStartedPlacingBuilding;
        Building.OnBuildingInited -= OnBuildingInited;
    }

    private void Show()
    {
        content.SetActive(true);
    }

    private void Hide()
    {
        content.SetActive(false);
    }

    private void OnButtonClicked()
    {
        EventBus.InvokeConstructionStopped();
        Hide();
    }

    private void OnStartedPlacingBuilding(Building building)
    {
        Show();
    }

    private void OnBuildingInited(Building building)
    {
        Hide();
    }
}
