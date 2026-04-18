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

    protected override void OnBuildingStartWorking()
    {
        base.OnBuildingStartWorking();

        StartPlayingSmoke();
    }

    protected override void OnBuildingStopWorking()
    {
        base.OnBuildingStartWorking();

        StopPlayingSmoke();
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
