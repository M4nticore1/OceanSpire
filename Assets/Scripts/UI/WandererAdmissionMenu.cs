using UnityEngine;

public class WandererAdmissionMenu : MonoBehaviour
{
    private Human selectedHuman;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private SkillsPanel skillPanel;
    [SerializeField] private CustomButton acceptButton;
    [SerializeField] private CustomButton rejectButton;

    private bool isOpened = false;

    private void OnEnable()
    {
        slidePanel.onClosed += OnClosed;
        acceptButton.onReleased += OnAcceptButtonClicked;
        rejectButton.onReleased += OnRejectButtonClicked;

        Boat.onBoatSelected += OnBoatSelected;
        Boat.onBoatDeselected += OnBoatDeselected;
    }

    private void OnDisable()
    {
        slidePanel.onClosed -= OnClosed;
        acceptButton.onReleased -= OnAcceptButtonClicked;
        rejectButton.onReleased -= OnRejectButtonClicked;

        Boat.onBoatSelected -= OnBoatSelected;
        Boat.onBoatDeselected -= OnBoatDeselected;
    }

    public void Open(Human human)
    {
        selectedHuman = human;
        slidePanel.Open();

        skillPanel.SetSkills(human.SkillsComponent);

        isOpened = true;
    }

    private void Close()
    {
        slidePanel.Close();
        OnClosed();
        isOpened = false;
    }

    private void OnClosed()
    {
        if (!isOpened) return;

        Human lastHuman = selectedHuman;
        selectedHuman = null;
        BoatRider rider = lastHuman.BoatRider;
        Boat boat = rider.selectedBoat;

        if (boat) {
            boat.SelectComponent.SetClickable(true);
            boat.SelectComponent.Deselect();
        }

        isOpened = false;
    }

    private void OnAcceptButtonClicked()
    {
        selectedHuman.AcceptWanderer();
        Close();
    }

    private void OnRejectButtonClicked()
    {
        selectedHuman.RejectWanderer();
        Close();
    }

    private void OnBoatSelected(Boat boat)
    {
        if (!boat.currentRider) return;

        Human human = boat.currentRider.GetComponent<Human>();
        if (human.currentStatusEnum != HumanStatusEnum.Wanderer) return;

        Open(human);
    }

    private void OnBoatDeselected(Boat boat)
    {
        if (!selectedHuman) return;

        Close();
    }
}