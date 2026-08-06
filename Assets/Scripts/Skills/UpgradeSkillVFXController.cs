using UnityEngine;

public class UpgradeSkillVFXController : VFXController
{
    [SerializeField] private ParticleSystem upgradeSkillVFX;
    [SerializeField] private Vector3 spawnVfxPositionOffset = new Vector3(0f, 1f, 0f);

    protected override void Subscribe()
    {
        base.Subscribe();

        UpgradeSkillsController.OnSkillsUpgraded += OnSkillUpgraded;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        UpgradeSkillsController.OnSkillsUpgraded -= OnSkillUpgraded;
    }

    private void OnSkillUpgraded(UpgradeSkillsController controller)
    {
        Instantiate(upgradeSkillVFX, controller.transform.position + spawnVfxPositionOffset, Quaternion.identity);
    }
}