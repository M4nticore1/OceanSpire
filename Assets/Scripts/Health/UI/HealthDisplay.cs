using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private HealthComponent health;

    [Header("Display")]
    [SerializeField] private GameObject content;
    [SerializeField] private TextLocalizer healthTextLocalizer;
    [SerializeField] private Image bar;
    [SerializeField] private Gradient barGradient;

    [Header("Stats")]
    [SerializeField] private float minHealthVisibilityThreshold = 0.5f;
    [SerializeField] private float visibilityTime = 0f;
    private float currentVisibilityTime = 0f;

    private bool isDisplayed = false;
    private bool isSubscribed = false;

    private void OnEnable()
    {
        if (!ShouldSubscribe()) return;

        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        if (!content) return;
        if (!health) return;

        float currentHealth = health.CurrentHealth;
        float maxHealth = health.MaxHealth;
        float alpha = currentHealth / maxHealth;

        if (alpha < minHealthVisibilityThreshold) {
            Display();
        }
        else {
            Hide();
        }
    }

    private void Update()
    {
        if (!content) return;

        if (isDisplayed) {
            currentVisibilityTime += Time.deltaTime;

            if (currentVisibilityTime > visibilityTime) {
                Hide();
            }
        }
    }

    public void SetHealthComponent(HealthComponent health)
    {
        Unsubscribe();

        this.health = health;

        if (ShouldSubscribe()) {
            Subscribe();
        }
    }

    private void OnHealthChanged()
    {
        TryToDisplay();
        TryUpdateHealth();
        ResetVisibilityTime();
    }

    private void OnDied()
    {
        Hide();
    }

    private bool TryToDisplay()
    {
        float currentHealth = health.CurrentHealth;
        float maxHealth = health.MaxHealth;
        float alpha = currentHealth / maxHealth;

        if (!isDisplayed && alpha <= minHealthVisibilityThreshold) {
            Display();
            return true;
        }

        return false;
    }

    private void Display()
    {
        content.SetActive(true);
        isDisplayed = true;
    }

    private void Hide()
    {
        content.SetActive(false);
        isDisplayed = false;
    }

    private void ResetVisibilityTime()
    {
        currentVisibilityTime = 0;
    }

    private void TryUpdateHealth()
    {
        if (!isDisplayed) return;

        UpdateHealth();
    }

    public void UpdateHealth()
    {
        if (healthTextLocalizer) {
            UpdateHealthText();
        }

        if (bar) {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthText()
    {
        healthTextLocalizer.SetPlaceHolderLocalization(health);
        healthTextLocalizer.UpdateText();
    }

    private void UpdateHealthBar()
    {
        float currentHealth = health.CurrentHealth;
        float maxHealth = health.MaxHealth;
        float alpha = currentHealth > 0 ? currentHealth / maxHealth : 0f;
        Color color = barGradient.Evaluate(alpha);

        bar.fillAmount = alpha;
        bar.color = color;
    }

    private void Subscribe()
    {
        if (!health) return;

        health.OnHealthChanged += OnHealthChanged;
        health.OnDied += OnDied;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!health) return;

        health.OnHealthChanged -= OnHealthChanged;
        health.OnDied -= OnDied;
        isSubscribed = false;
    }

    private bool ShouldSubscribe()
    {
        if (isSubscribed) return false;
        if (!health) return false;

        return true;
    }
}