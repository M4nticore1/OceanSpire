using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private Image bar;
    [SerializeField] private GameObject content;

    private bool isDisplayed = false;
    [SerializeField] private float minHealthVisibilityThreshold = 0.5f;
    [SerializeField] private float timeToHide = 10f;
    private float currentTimeToHide = 0f;

    private void OnEnable()
    {
        healthComponent.onHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        healthComponent.onHealthChanged += OnHealthChanged;
    }

    private void Start()
    {
        float currentHealth = healthComponent.CurrentHealth;
        float maxHealth = healthComponent.MaxHealth;
        float alpha = currentHealth / maxHealth;
        if (alpha <= minHealthVisibilityThreshold)
            Display();
        else
            Hide();
    }

    private void Update()
    {
        if (isDisplayed && timeToHide > 0) {
            if (currentTimeToHide < timeToHide) {
                currentTimeToHide += Time.deltaTime;
                if (currentTimeToHide >= timeToHide) {
                    Hide();
                    ResetTime();
                }
            }
        }
    }

    private void OnHealthChanged()
    {
        float currentHealth = healthComponent.CurrentHealth;
        float maxHealth = healthComponent.MaxHealth;
        float alpha = currentHealth / maxHealth;
        if (!isDisplayed && alpha <= minHealthVisibilityThreshold) {
            Display();
            SetHealth(currentHealth, maxHealth);
        }
        else {

        }
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

    private void ResetTime()
    {
        currentTimeToHide = 0;
    }

    private void SetHealth(float currentHealth, float maxHealth)
    {
        bar.fillAmount = maxHealth / currentHealth;
    }
}
