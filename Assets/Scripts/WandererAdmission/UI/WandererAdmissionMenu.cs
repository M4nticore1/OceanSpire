using System;
using UnityEngine;

public class WandererAdmissionMenu : MonoBehaviour, IOpenable
{
    [Header("Main")]
    [SerializeField] private WandererAdmissionManager wandererAdmissionManager;

    [Header("UI")]
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private SkillsPanel skillPanel;
    [SerializeField] private CustomButton acceptButton;
    [SerializeField] private CustomButton rejectButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private TextLocalizer wandererNameText;

    private Wanderer selectedWanderer;

    public bool IsShown { get; private set; } = false;

    public event Action OnShown;
    public event Action OnHidden;

    private void OnEnable()
    {
        slidePanel.OnHidden += HandleHidden;
        acceptButton.OnStateChanged += OnAcceptButtonStateChanged;
        acceptButton.OnReleased.AddListener(OnAcceptButtonClicked);
        rejectButton.OnReleased.AddListener(OnRejectButtonClicked);
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);

        Human.OnHumanDied += OnHumanDied;

        Boat.OnBoatSelected += OnBoatSelected;
        Boat.OnBoatDeselected += OnBoatDeselected;
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= HandleHidden;
        acceptButton.OnStateChanged -= OnAcceptButtonStateChanged;
        acceptButton.OnReleased.RemoveListener(OnAcceptButtonClicked);
        rejectButton.OnReleased.RemoveListener(OnRejectButtonClicked);
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);

        Human.OnHumanDied -= OnHumanDied;

        Boat.OnBoatSelected -= OnBoatSelected;
        Boat.OnBoatDeselected -= OnBoatDeselected;
    }

    public void Show()
    {
        UpdateAcceptButtonEnabled();
        OnShown?.Invoke();
    }

    public void Show(Wanderer wanderer)
    {
        if (!wanderer) {
            Debug.LogError("Wanderer is not valid");
            return;
        }

        IsShown = true;
        slidePanel.Show();
        selectedWanderer = wanderer;

        UpdateWandererNameText();
        UpdateSkillsPanel();

        InputStateManager.Instance.AddInputBlockTarget(this);

        Show();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    private void HandleHidden()
    {
        if (!IsShown) return;

        IsShown = false;
        selectedWanderer.BoatRider.RidingBoat.SelectComponent.Deselect();
        InputStateManager.Instance.RemoveBlockTarget(this);
        OnHidden?.Invoke();
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
        acceptButton.SetState(wandererAdmissionManager.CanAcceptWanderer(selectedWanderer) ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void OnAcceptButtonClicked()
    {
        wandererAdmissionManager.AcceptWanderer(selectedWanderer);
        Hide();
    }

    private void OnRejectButtonClicked()
    {
        wandererAdmissionManager.RejectWanderer(selectedWanderer);
        Hide();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }

    private void OnAcceptButtonStateChanged(CustomButtonState state)
    {
        if (state == CustomButtonState.Disabled) {
            UpdateAcceptButtonEnabled();
        }
    }

    private void OnHumanDied(Human human)
    {
        var citizen = human as Citizen;
        if (!citizen) return;

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