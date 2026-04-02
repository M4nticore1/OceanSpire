using UnityEngine;

public class WandererAdmissionMenu : MonoBehaviour
{
    private Human selectedHuman;
    [SerializeField] private SlidePanel slidePanel;
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
        Boat boat = rider.currentBoat;

        if (boat) {
            boat.SelectComponent.SetClickable(true);
            boat.SelectComponent.SetSelected(false);
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
        if (!boat.rider) return;

        Human human = boat.rider.GetComponent<Human>();
        if (human.currentStateEnum != HumanStateEnum.Wanderer) return;

        Open(human);
    }

    private void OnBoatDeselected(Boat boat)
    {
        if (!selectedHuman) return;

        Close();
    }
}