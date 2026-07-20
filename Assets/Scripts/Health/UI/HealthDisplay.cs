using System.Collections;
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

    private Coroutine hideCoroutine;

    private bool isDisplayed = false;
    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
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

    private void TrySubscribe()
    {
        if (!ShouldSubscribe()) return;

        health.OnHealthChanged += OnHealthChanged;
        health.OnDied += OnDied;
        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!ShouldUnsubscribe()) return;

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

    private bool ShouldUnsubscribe()
    {
        if (!isSubscribed) return false;
        if (!health) return false;

        return true;
    }

    public void SetHealthComponent(HealthComponent health)
    {
        TryUnsubscribe();

        this.health = health;

        TrySubscribe();
    }

    public void RemoveHealthComponent()
    {
        TryUnsubscribe();
        health = null;
    }

    private void OnHealthChanged()
    {
        TryToDisplay();
        TryUpdateHealth();
        UpdateHideCoroutine();
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

    public void Display()
    {
        if (!content) return;

        content.SetActive(true);
        isDisplayed = true;
    }

    public void Hide()
    {
        if (!content) return;

        if (hideCoroutine != null) {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        content.SetActive(false);
        isDisplayed = false;
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

    private void UpdateHideCoroutine()
    {
        if (hideCoroutine != null) {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (visibilityTime <= 0) {
            return;
        }

        hideCoroutine = StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        yield return new WaitForSeconds(visibilityTime);

        Hide();
        hideCoroutine = null;
    }
}