using UnityEngine;

public class WandererAdmissionMenu : MonoBehaviour
{
    private Wanderer selectedWanderer;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private SkillsPanel skillPanel;
    [SerializeField] private CustomButton acceptButton;
    [SerializeField] private CustomButton rejectButton;
    [SerializeField] private TextLocalizer wandererNameText;

    private bool isOpened = false;

    private void OnEnable()
    {
        slidePanel.OnClosed += OnClosed;
        acceptButton.OnReleased.AddListener(OnAcceptButtonClicked);
        rejectButton.OnReleased.AddListener(OnRejectButtonClicked);

        Human.OnHumanDied += OnHumanDied;

        Boat.OnBoatSelected += OnBoatSelected;
        Boat.OnBoatDeselected += OnBoatDeselected;

        UpdateAcceptButtonEnabled();
    }

    private void OnDisable()
    {
        slidePanel.OnClosed -= OnClosed;
        acceptButton.OnReleased.RemoveListener(OnAcceptButtonClicked);
        rejectButton.OnReleased.RemoveListener(OnRejectButtonClicked);

        Human.OnHumanDied -= OnHumanDied;

        Boat.OnBoatSelected -= OnBoatSelected;
        Boat.OnBoatDeselected -= OnBoatDeselected;
    }

    public void Open(Wanderer wanderer)
    {
        if (!wanderer) {
            Debug.LogError("Wanderer is not valid");
            return;
        }

        isOpened = true;
        slidePanel.Open();
        selectedWanderer = wanderer;

        UpdateWandererNameText();
        UpdateSkillsPanel();

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
        selectedWanderer.BoatRider.RidingBoat.SelectComponent.Deselect();
        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }

    private void UpdateWandererNameText()
    {
        wandererNameText.SetPlaceHolderLocalization(selectedWanderer.NameComponent);
    }

    private void UpdateSkillsPanel()
    {
        skillPanel.SetSkills(selectedWanderer.SkillsComponent);
    }

    private void UpdateAcceptButtonEnabled()
    {
        var currentPopulation = CreaturesManager.Instance.Citizens.Count;
        var maxPopulation = CityStorage.Instance.Inventory.GetItemById(ItemID.Population);
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

    private void OnHumanDied(Human human)
    {
        var citizen = human as Citizen;
        if (citizen == null) return;

        UpdateAcceptButtonEnabled();
    }

    private void OnBoatSelected(Boat boat)
    {
        if (!boat.CurrentRider) return;

        var wanderer = boat.CurrentRider.GetComponent<Wanderer>();
        if (!wanderer) return;

        if (wanderer.IsRejected) return;

        Open(wanderer);
    }

    private void OnBoatDeselected(Boat boat)
    {
        if (selectedWanderer) return;

        Close();
    }
}