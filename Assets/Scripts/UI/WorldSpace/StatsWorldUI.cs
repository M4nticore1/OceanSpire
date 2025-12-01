using UnityEngine;
using UnityEngine.UI;

public class StatsWorldUI : MonoBehaviour
{
    [SerializeField] private GameObject healthBar = null;
    [SerializeField] private Image healthBarFill = null;
    [SerializeField] private GameObject actionProgressBar = null;
    [SerializeField] private Image actionProgressBarFill = null;

    public bool isHealthBarShowed { get; private set; } = false;
    public bool isActionProgressBarShowed { get; private set; } = false;

    public void Initialize(float currentHealth, float maxHealth, float healthDisplayThreshold)
    {
        if (healthBar)
        {
            if (currentHealth <= maxHealth * healthDisplayThreshold)
            {
                ShowHealthBar();
                SetHealthBarAlpha(currentHealth / maxHealth);
            }
            else
            {
                HideHealthBar();
            }
        }

        if (actionProgressBarFill)
        {
            HideActionProgressBar();
        }
    }

    public void SetHealthBarAlpha(float alpha)
    {
        healthBarFill.fillAmount = alpha;
    }

    public void ShowHealthBar()
    {
        healthBar.SetActive(true);
        isHealthBarShowed = true;
    }

    public void HideHealthBar()
    {
        healthBar.SetActive(false);
        isHealthBarShowed = false;
    }

    public void ShowActionProgressBar()
    {
        isActionProgressBarShowed = true;
        actionProgressBar.gameObject.SetActive(true);
    }

    public void HideActionProgressBar()
    {
        isActionProgressBarShowed = false;
        actionProgressBar.gameObject.SetActive(false);
    }

    public void SetActionProgressFillAmount(float alpha)
    {
        actionProgressBarFill.fillAmount = alpha;
    }
}
