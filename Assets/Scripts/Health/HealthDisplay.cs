using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Health healthComponent;

    [Header("Display")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image bar;
    [SerializeField] private bool addHealthText;
    [SerializeField] private GameObject content;

    [Header("Stats")]
    [SerializeField] private float minHealthVisibilityThreshold = 0.5f;
    [SerializeField] private float timeToHide = 0f;
    private float currentTimeToHide = 0f;

    private bool isDisplayed = false;

    private void OnEnable()
    {
        if (healthComponent) {
            healthComponent.onHealthChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (healthComponent) {
            healthComponent.onHealthChanged -= OnHealthChanged;
        }
    }

    private void Start()
    {
        float currentHealth = healthComponent.CurrentHealth;
        float maxHealth = healthComponent.MaxHealth;
        float alpha = currentHealth / maxHealth;

        if (alpha <= minHealthVisibilityThreshold) {
            Display();
        }
        else {
            Hide();
        }
    }

    private void Update()
    {
        if (!isDisplayed) return;
        if (timeToHide <= 0) return;
        if (currentTimeToHide > timeToHide) return;

        currentTimeToHide += Time.deltaTime;

        if (currentTimeToHide < timeToHide) return;

        Hide();
        ResetTime();
    }

    public void SetHealthComponent(Health health)
    {
        if (healthComponent) {
            healthComponent.onHealthChanged -= OnHealthChanged;
        }

        healthComponent = health;
        healthComponent.onHealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged()
    {
        float currentHealth = healthComponent.CurrentHealth;
        float maxHealth = healthComponent.MaxHealth;
        float alpha = currentHealth / maxHealth;

        if (!isDisplayed && alpha <= minHealthVisibilityThreshold) {
            Display();
        }

        SetHealth(currentHealth, maxHealth);
    }

    private void Display()
    {
        if (content) {
            content.SetActive(true);
        }
        
        isDisplayed = true;
    }

    private void Hide()
    {
        if (content) {
            content.SetActive(false);
        }

        isDisplayed = false;
    }

    private void ResetTime()
    {
        currentTimeToHide = 0;
    }

    private void SetHealth(float currentHealth, float maxHealth)
    {
        if (healthText) {
            AssignHealthText(currentHealth, maxHealth);
        }

        if (bar) {
            AssignHealthBar(currentHealth, maxHealth);
        }
    }

    private void AssignHealthText(float currentHealth, float maxHealth)
    {
        string text = math.ceil(currentHealth).ToString() + "/" + maxHealth.ToString();
        healthText.SetText(text);
    }

    private void AssignHealthBar(float currentHealth, float maxHealth)
    {
        float fillAmount = currentHealth > 0 ? maxHealth / currentHealth : 0f;
        bar.fillAmount = fillAmount;
    }
}
