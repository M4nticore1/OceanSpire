using UnityEngine;

public class BoatingSkillProgress : SkillProgress
{
    [SerializeField] private float gainXpFrequency = 10f;

    private float currentAddXpTime;

    private void Update()
    {
        currentAddXpTime += Time.deltaTime;
        if (currentAddXpTime < gainXpFrequency) return;

        foreach (var component in SkillAdapter.SkillComponents) {
            if (!ShouldAddXp(component)) continue;

            float xp = XpGain * gainXpFrequency;
            AddXp(xp);
        }

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

    private bool ShouldAddXp(SkillsComponent skillsComponent)
    {
        var boatRider = skillsComponent.GetComponent<BoatRider>();
        if (!boatRider.RidingBoat) return false;

        return true;
    }
}