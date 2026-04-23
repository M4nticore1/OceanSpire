using UnityEngine;

public class CoalGeneratorModule : ProductionModule
{
    [Header("Coal Generator")]
    [SerializeField] private ParticleSystem smokePrefab = null;
    private ParticleSystem spawnedSmoke = null;
    //[SerializeField] private Gradient smokeGradient = null;

    CoalGenetatorConstructionModule CoalGenetatorConstructionModule => BuildingConstruction ? BuildingConstruction.GetComponent<CoalGenetatorConstructionModule>() : null;
    TimerHandle stopProductingTimerHandle = new TimerHandle();

    protected override void OnInit()
    {
        base.OnInit();

    }

    protected override void OnBuildingStartWorking()
    {
        base.OnBuildingStartWorking();

        TrySpawnSmoke();
        StartPlayingSmoke();
    }

    protected override void OnBuildingStopWorking()
    {
        base.OnBuildingStartWorking();

        StopPlayingSmoke();
    }

    private void TrySpawnSmoke()
    {
        if (spawnedSmoke) return;

        Transform smokeTransform = CoalGenetatorConstructionModule.SmokeSpawnTransform;
        spawnedSmoke = Instantiate(smokePrefab);
        spawnedSmoke.transform.position = smokeTransform.position;
        spawnedSmoke.transform.SetParent(transform);
        spawnedSmoke.gameObject.SetActive(false);
    }

    private void StartPlayingSmoke()
    {
        TimerManager.Instance.RemoveTimer(stopProductingTimerHandle);
        spawnedSmoke.gameObject.SetActive(true);
        spawnedSmoke.Play();
    }

    private void StopPlayingSmoke()
    {
        spawnedSmoke.Stop();
        float time = smokePrefab.main.startLifetime.constant;
        TimerManager.Instance.StartTimer(stopProductingTimerHandle, time, () => spawnedSmoke.gameObject.SetActive(false));
    }
}
