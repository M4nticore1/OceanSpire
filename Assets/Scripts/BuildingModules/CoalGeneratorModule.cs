using UnityEngine;

public class CoalGeneratorModule : ProductionModule
{
    [Header("Coal Generator")]
    [SerializeField] private ParticleSystem smokePrefab = null;
    private ParticleSystem spawnedSmoke = null;
    //[SerializeField] private Gradient smokeGradient = null;

    CoalGenetatorConstructionModule CoalGenetatorConstructionModule => BuildingConstruction.GetComponent<CoalGenetatorConstructionModule>();
    TimerHandle stopProductingTimerHandle = new TimerHandle();

    protected override void OnInit()
    {
        base.OnInit();

        Transform smokeTransform = CoalGenetatorConstructionModule.SmokeSpawnTransform;
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
