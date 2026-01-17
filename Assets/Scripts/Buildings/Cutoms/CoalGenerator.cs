using UnityEngine;

public class CoalGenerator : ProductionBuilding
{
    [Header("Coal Generator")]
    [SerializeField] private ParticleSystem smokePrefab = null;
    private ParticleSystem spawnedSmoke = null;
    //[SerializeField] private Gradient smokeGradient = null;

    CoalGenetatorConstruction CoalGenetatorConstruction => (CoalGenetatorConstruction)BuildingConstruction;
    TimerHandle stopProductingTimerHandle = new TimerHandle();

    protected override void BuildComponent()
    {
        base.BuildComponent();
        Transform smokeTransform = CoalGenetatorConstruction.SmokeSpawnTransform;
        spawnedSmoke = Instantiate(smokePrefab);
        spawnedSmoke.transform.position = smokeTransform.position;
        spawnedSmoke.transform.SetParent(transform);
        spawnedSmoke.gameObject.SetActive(false);
    }

    protected override void OnStartProducting()
    {
        base.OnStartProducting();
        TimerManager.RemoveTimer(stopProductingTimerHandle);
        spawnedSmoke.gameObject.SetActive(true);
        spawnedSmoke.Play();
        //SetSmokeColor();
    }

    protected override void OnStopProducting()
    {
        base.OnStopProducting();
        StopPlayingSmoke();
    }

    private void SetSmokeColor()
    {
        //var main = spawnedSmoke.main;
        //main.startColor = smokeGradient.Evaluate(producedItem.Amount / producingItem.maxResourceAmount);
    }

    private void StopPlayingSmoke()
    {
        spawnedSmoke.Stop();
        float time = smokePrefab.main.startLifetime.constant;
        TimerManager.StartTimer(stopProductingTimerHandle, time, () => spawnedSmoke.gameObject.SetActive(false));
    }
}
