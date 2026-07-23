using System;
using UnityEngine;

public class EvictMenu : MonoBehaviour, IOpenable
{
    [SerializeField] EvictManager evictManager;

    [SerializeField] private CustomButton evictButton;
    [SerializeField] private CustomButton closeButton;

    [SerializeField] private SkillsPanel skilsPanel;
    [SerializeField] private SlidePanel slidePanel;

    [SerializeField] private TextLocalizer citizenNameText;

    private Citizen SelectedCitizen;

    public bool IsShowed { get; private set; } = false;

    public event Action OnShowed;
    public event Action OnHidden;

    private void OnEnable()
    {
        evictButton.OnReleased.AddListener(OnEvictButtonClicked);
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
        Human.OnHumanDied += OnHumanDied;
    }

    private void OnDisable()
    {
        evictButton.OnReleased.RemoveListener(OnEvictButtonClicked);
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
        Human.OnHumanDied -= OnHumanDied;
    }

    public void Show()
    {
        IsShowed = true;
        slidePanel.Show();

        var citizen = SelectManager.Instance.GetSelectedHuman() as Citizen;
        if (!citizen) return;

        SelectedCitizen = citizen;

        UpdateCitizenName(citizen);
        UpdateSkills(citizen);
        UpdateEvictButtonEnabled(citizen);

        InputStateManager.Instance.AddBlockTarget(this);

        OnShowed?.Invoke();
    }

    public void Hide()
    {
        IsShowed = false;
        slidePanel.Hide();
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
    }

    private void UpdateCitizenName(Citizen citizen)
    {
        citizenNameText.SetPlaceHolderLocalization(citizen.NameComponent);
    }

    private void UpdateSkills(Citizen citizen)
    {
        skilsPanel.SetSkills(citizen.SkillsComponent);
    }

    private void UpdateEvictButtonEnabled(Citizen citizen)
    {
        bool shouldEnable = citizen && citizen.HealthComponent.IsAlive;
        evictButton.SetState(shouldEnable ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void OnEvictButtonClicked()
    {
        evictManager.TryEvictCitizen(SelectedCitizen);
        Hide();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }

    private void OnHumanDied(Human human)
    {
        if (human != SelectedCitizen) return;

        UpdateEvictButtonEnabled(SelectedCitizen);
    }
}