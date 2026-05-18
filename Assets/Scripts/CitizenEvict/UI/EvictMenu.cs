using UnityEngine;
using UnityEngine.EventSystems;

public class EvictMenu : UIBehaviour
{
    [SerializeField] EvictManager evictManager;

    [SerializeField] private CustomButton evictButton;
    [SerializeField] private CustomButton closeButton;

    [SerializeField] private SkillsPanel skilsPanel;
    [SerializeField] private SlidePanel slidePanel;

    private Citizen SelectedCitizen;

    protected override void OnEnable()
    {
        base.OnEnable();

        evictButton.OnReleased.AddListener(OnEvictButtonClicked);
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
        Human.OnHumanDied += OnHumanDied;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        evictButton.OnReleased.RemoveListener(OnEvictButtonClicked);
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
        Human.OnHumanDied -= OnHumanDied;
    }

    public void Open(Citizen citizen)
    {
        SelectedCitizen = citizen;
        UpdateEvictButtonEnabled();
        skilsPanel.SetSkills(citizen.SkillsComponent);
    }

    private void Close()
    {
        slidePanel.Close();
    }

    private void UpdateEvictButtonEnabled()
    {
        bool shouldEnable = SelectedCitizen && SelectedCitizen.HealthComponent.IsAlive;
        evictButton.SetState(shouldEnable ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void OnEvictButtonClicked()
    {
        evictManager.TryEvict(SelectedCitizen);
    }

    private void OnCloseButtonClicked()
    {
        Close();
    }

    private void OnHumanDied(Human human)
    {
        if (human != SelectedCitizen) return;

        UpdateEvictButtonEnabled();
    }
}