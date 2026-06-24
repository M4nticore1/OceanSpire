using UnityEngine;

public class BoatingSkillProgress : SkillProgress
{
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private float addXpFrequency;

    private float currentAddXpTime;

    private void Update()
    {
        currentAddXpTime += Time.deltaTime;
        if (currentAddXpTime < addXpFrequency) return;

        foreach (var citizen in creaturesManager.Citizens) {
            var boatRider = citizen.BoatRider;
            if (!ShouldAddXp(boatRider)) continue;

            AddXp();
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

    private bool ShouldAddXp(BoatRider rider)
    {
        if (!rider.RidingBoat) return false;

        return true;
    }
}