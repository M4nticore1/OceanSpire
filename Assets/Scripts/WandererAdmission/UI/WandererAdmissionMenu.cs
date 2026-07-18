using System;
using UnityEngine;

public class WandererAdmissionMenu : MonoBehaviour, IOpenable
{
    [Header("Main")]
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private CreaturesManager creaturesManager;

    [Header("UI")]
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private SkillsPanel skillPanel;
    [SerializeField] private CustomButton acceptButton;
    [SerializeField] private CustomButton rejectButton;
    [SerializeField] private TextLocalizer wandererNameText;

    private bool isOpened = false;
    private Wanderer selectedWanderer;

    public event Action OnShown;
    public event Action OnHidden;

    private void OnEnable()
    {
        slidePanel.OnHidden += OnClosed;
        acceptButton.OnReleased.AddListener(OnAcceptButtonClicked);
        rejectButton.OnReleased.AddListener(OnRejectButtonClicked);

        Human.OnHumanDied += OnHumanDied;

        Boat.OnBoatSelected += OnBoatSelected;
        Boat.OnBoatDeselected += OnBoatDeselected;

        UpdateAcceptButtonEnabled();
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= OnClosed;
        acceptButton.OnReleased.RemoveListener(OnAcceptButtonClicked);
        rejectButton.OnReleased.RemoveListener(OnRejectButtonClicked);

        Human.OnHumanDied -= OnHumanDied;

        Boat.OnBoatSelected -= OnBoatSelected;
        Boat.OnBoatDeselected -= OnBoatDeselected;
    }

    public void Show()
    {
        OnShown?.Invoke();
    }

    public void Show(Wanderer wanderer)
    {
        if (!wanderer) {
            Debug.LogError("Wanderer is not valid");
            return;
        }

        isOpened = true;
        slidePanel.Show();
        selectedWanderer = wanderer;

        UpdateWandererNameText();
        UpdateSkillsPanel();

        InputStateManager.Instance.SetGameplayInputBlocked(true);

        Show();
    }

    public void Hide()
    {
        slidePanel.Hide();
        OnClosed();

        OnHidden?.Invoke();
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
        var currentPopulation = creaturesManager.Citizens.Count;
        var maxPopulation = cityStorage.Inventory.GetItem(ItemID.Population);
    }

    private void OnAcceptButtonClicked()
    {
        WandererAdmissionSystem.AcceptWanderer(selectedWanderer);
        Hide();
    }

    private void OnRejectButtonClicked()
    {
        WandererAdmissionSystem.RejectWanderer(selectedWanderer);
        Hide();
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

        Show(wanderer);
    }

    private void OnBoatDeselected(Boat boat)
    {
        if (selectedWanderer) return;

        Hide();
    }
}