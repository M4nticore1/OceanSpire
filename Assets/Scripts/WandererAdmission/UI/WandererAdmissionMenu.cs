using UnityEngine;

public class WandererAdmissionMenu : MonoBehaviour
{
    private Wanderer selectedWanderer;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private SkillsPanel skillPanel;
    [SerializeField] private CustomButton acceptButton;
    [SerializeField] private CustomButton rejectButton;

    private bool isOpened = false;

    private void OnEnable()
    {
        slidePanel.onClosed += OnClosed;
        acceptButton.onReleased.AddListener(OnAcceptButtonClicked);
        rejectButton.onReleased.AddListener(OnRejectButtonClicked);

        Boat.onBoatSelected += OnBoatSelected;
        Boat.onBoatDeselected += OnBoatDeselected;
    }

    private void OnDisable()
    {
        slidePanel.onClosed -= OnClosed;
        acceptButton.onReleased.RemoveListener(OnAcceptButtonClicked);
        rejectButton.onReleased.RemoveListener(OnRejectButtonClicked);

        Boat.onBoatSelected -= OnBoatSelected;
        Boat.onBoatDeselected -= OnBoatDeselected;
    }

    public void Open()
    {
        isOpened = true;

        slidePanel.Open();
        skillPanel.SetSkills(selectedWanderer.SkillsComponent);

        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    private void Close()
    {
        slidePanel.Close();
        OnClosed();
    }

    private void OnClosed()
    {
        if (!isOpened) return;

        isOpened = false;
        selectedWanderer.BoatRider.SelectedBoat.SelectComponent.Deselect();
        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }

    private void OnAcceptButtonClicked()
    {
        WandererAdmissionSystem.AcceptWanderer(selectedWanderer);
        Close();
    }

    private void OnRejectButtonClicked()
    {
        WandererAdmissionSystem.RejectWanderer(selectedWanderer);
        Close();
    }

    private void OnBoatSelected(Boat boat)
    {
        if (!boat.SelectedRider) return;
        var wanderer = boat.SelectedRider.GetComponent<Wanderer>();

        if (wanderer.CurrentStatusEnum != HumanStatusEnum.Wanderer) return;
        if (wanderer.IsRejected) return;

        selectedWanderer = wanderer;
        Open();
    }

    private void OnBoatDeselected(Boat boat)
    {
        if (selectedWanderer) return;

        Close();
    }
}