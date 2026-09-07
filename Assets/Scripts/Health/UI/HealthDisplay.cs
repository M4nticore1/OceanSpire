using System.Collections;
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
    [SerializeField, Min(0f)] private float minHealthVisibilityThreshold = 0.5f;
    [SerializeField, Min(0f)] private float visibilityTime = 0f;

    private Coroutine hideCoroutine;
    private bool isDisplayed => content != null ? content.activeSelf && content.activeInHierarchy : true;
    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
        StopHideCoroutine();
    }

    private void Start()
    {
        UpdateDisplayed();
    }

    public void SetHealthComponent(HealthComponent health)
    {
        if (this.health == health)
            return;

        TryUnsubscribe();

        this.health = health;

        TrySubscribe();
        UpdateHealth();
    }

    public void RemoveHealthComponent()
    {
        TryUnsubscribe();

        health = null;
        Hide();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        if (health == null)
            return;

        health.OnHealthChanged += OnHealthChanged;
        health.OnDied += OnDied;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed)
            return;

        if (health == null) {
            isSubscribed = false;
            return;
        }

        health.OnHealthChanged -= OnHealthChanged;
        health.OnDied -= OnDied;

        isSubscribed = false;
    }

    private void OnHealthChanged()
    {
        if (health == null)
            return;

        TryDisplay();
        UpdateHealth();
        UpdateHideCoroutine();
    }

    private void OnDied()
    {
        Hide();
    }

    private bool TryDisplay()
    {
        if (health == null)
            return false;

        if (isDisplayed)
            return false;

        if (GetHealthPercent() > minHealthVisibilityThreshold)
            return false;

        Display();

        return true;
    }

    public void Display()
    {
        if (content == null) {
            content.SetActive(true);
        }

        UpdateHealth();
    }

    public void Hide()
    {
        StopHideCoroutine();

        if (content != null) {
            content.SetActive(false);
        }
    }

    public void UpdateHealth()
    {
        if (!isDisplayed)
            return;

        UpdateHealthText();
        UpdateHealthBar();
    }

    private void UpdateDisplayed()
    {
        if (ShouldDisplay()) {
            Display();
        }
        else {
            Hide();
        }
    }

    private void UpdateHealthText()
    {
        if (health == null)
            return;

        if (healthTextLocalizer == null)
            return;

        healthTextLocalizer.SetPlaceHolderLocalization(health);
    }

    private void UpdateHealthBar()
    {
        if (health == null)
            return;

        if (bar == null)
            return;

        var healthPercent = GetHealthPercent();
        bar.fillAmount = healthPercent;

        if (barGradient != null) {
            bar.color = barGradient.Evaluate(healthPercent);
        }
    }

    private void UpdateHideCoroutine()
    {
        StopHideCoroutine();

        if (visibilityTime <= 0f)
            return;

        hideCoroutine = StartCoroutine(HideDelayed());
    }

    private void StopHideCoroutine()
    {
        if (hideCoroutine == null)
            return;

        StopCoroutine(hideCoroutine);
        hideCoroutine = null;
    }

    private bool ShouldDisplay()
    {
        return GetHealthPercent() < minHealthVisibilityThreshold;
    }

    private float GetHealthPercent()
    {
        if (health == null)
            return 0f;

        if (health.MaxHealth <= 0f)
            return 0f;

        return Mathf.Clamp01(health.CurrentHealth / health.MaxHealth);
    }

    private IEnumerator HideDelayed()
    {
        yield return new WaitForSeconds(visibilityTime);

        hideCoroutine = null;
        Hide();
    }
}