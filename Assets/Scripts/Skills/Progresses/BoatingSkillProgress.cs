using UnityEngine;

public class BoatingSkillProgress : SkillProgress
{
    [SerializeField] private float gainXpFrequency = 10f;

    private float currentAddXpTime;

    private void Update()
    {
        currentAddXpTime += Time.deltaTime;
        if (currentAddXpTime < gainXpFrequency) return;

        AddXp(gainXpFrequency * XpGain);

        currentAddXpTime = 0f;
    }

    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        return true;
    }
}