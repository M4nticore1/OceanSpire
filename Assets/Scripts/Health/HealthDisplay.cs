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
    [SerializeField] private TextMeshProUGUI healthText;
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

        float currentHealth = health.currentHealth;
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
        TryAssignHealth();
        ResetVisibilityTime();
    }

    private void OnDied()
    {
        Hide();
    }

    private bool TryToDisplay()
    {
        float currentHealth = health.currentHealth;
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

    private void TryAssignHealth()
    {
        if (!isDisplayed) return;

        AssignHealth();
    }

    private void AssignHealth()
    {
        if (healthText) {
            AssignHealthText();
        }

        if (bar) {
            AssignHealthBar();
        }
    }

    private void AssignHealthText()
    {
        float currentHealth = health.currentHealth;
        float maxHealth = health.MaxHealth;

        string text = math.ceil(currentHealth).ToString() + "/" + maxHealth.ToString();
        healthText.SetText(text);
    }

    private void AssignHealthBar()
    {
        float currentHealth = health.currentHealth;
        float maxHealth = health.MaxHealth;
        float alpha = currentHealth > 0 ? currentHealth / maxHealth : 0f;
        Color color = barGradient.Evaluate(alpha);

        bar.fillAmount = alpha;
        bar.color = color;
    }

    private void Subscribe()
    {
        health.onHealthChanged += OnHealthChanged;
        health.onDied += OnDied;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        health.onHealthChanged -= OnHealthChanged;
        health.onDied -= OnDied;
        isSubscribed = false;
    }

    private bool ShouldSubscribe()
    {
        if (isSubscribed) return false;
        if (!health) return false;

        return true;
    }
}