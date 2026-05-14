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
        slidePanel.Open();
        skillPanel.SetSkills(selectedHuman.SkillsComponent);

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

        selectedHuman.BoatRider.SelectedBoat.SelectComponent.Deselect();
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
        if (!boat.SelectedRider) return;

        Human human = boat.SelectedRider.GetComponent<Human>();
        if (human.CurrentStatusEnum != HumanStatusEnum.Wanderer) return;

        selectedHuman = human;
        Open();
    }

    private void OnBoatDeselected(Boat boat)
    {
        if (selectedHuman) return;

        Close();
    }
}