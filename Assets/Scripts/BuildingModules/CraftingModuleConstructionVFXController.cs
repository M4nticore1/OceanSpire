using UnityEngine;

public class CraftingModuleConstructionVFXController : MonoBehaviour
{
    [SerializeField] private BuildingConstruction buildingConstruction;
    [SerializeField] private ParticleSystem vfx;

    private CraftingModule craftingModule = null;

    private bool isSubscribed = false;

    private void OnEnable()
    {
        Init();
        TrySubscribe();
        UpdatePlay();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        Init();
        TrySubscribe();
        UpdatePlay();
    }

    private void Init()
    {
        if (craftingModule != null) return;
        if (buildingConstruction == null) return;
        if (buildingConstruction.OwnedBuilding == null) return;

        craftingModule = buildingConstruction.OwnedBuilding.GetComponent<CraftingModule>();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!craftingModule) return;

        craftingModule.OnWorkingStarted += OnWorkStarted;
        craftingModule.OnWorkingStopped += OnWorkStopped;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!craftingModule) return;

        craftingModule.OnWorkingStarted -= OnWorkStarted;
        craftingModule.OnWorkingStopped -= OnWorkStopped;

        isSubscribed = false;
    }

    private void UpdatePlay()
    {
        if (!craftingModule) return;

        if (craftingModule.IsWorking) {
            PlayVFX();
        }
        else {
            StopVFX();
        }
    }

    private void PlayVFX()
    {
        if (!vfx) return;

        vfx.Play();
    }

    private void StopVFX()
    {
        if (!vfx) return;

        vfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void OnWorkStarted()
    {
        PlayVFX();
    }

    private void OnWorkStopped()
    {
        StopVFX();
    }
}