using UnityEngine;

public class SkillDescriptionManager : MonoBehaviour
{
    [SerializeField] private SkillDescriptionWidget skillDescriptionWidgetPrefab;

    private SkillDescriptionWidget spawnedSkillDescriptionWidget;

    private void OnEnable()
    {
        SkillWidget.OnSkillWidgetSelected += OnSkillWidgetSelected;
        SkillWidget.OnSkillWidgetDeselected += OnSkillWidgetDeselected;
    }

    private void OnDisable()
    {
        SkillWidget.OnSkillWidgetSelected -= OnSkillWidgetSelected;
        SkillWidget.OnSkillWidgetDeselected -= OnSkillWidgetDeselected;
    }

    private void OnSkillWidgetSelected(SkillWidget skillWidget)
    {
        if (!skillWidget) {
            Debug.LogError($"[{nameof(SkillDescriptionManager)}] Skill Widget is not valid");
            return;
        }
        
        if (spawnedSkillDescriptionWidget) {
            Destroy(spawnedSkillDescriptionWidget.gameObject);
            spawnedSkillDescriptionWidget = null;
        }

        SpawnDescriptionWidget(skillWidget);
    }

    private void OnSkillWidgetDeselected(SkillWidget skillWidget)
    {
        if (!skillWidget) return;
        if (skillWidget.Skill == null) return;

        if (!spawnedSkillDescriptionWidget) return;
        if (spawnedSkillDescriptionWidget.Skill != skillWidget.Skill) return;

        Destroy(spawnedSkillDescriptionWidget.gameObject);
        spawnedSkillDescriptionWidget = null;
    }

    private void SpawnDescriptionWidget(SkillWidget skillWidget)
    {
        if (!skillWidget) return;
        if (skillWidget.Skill == null) return;

        var skill = skillWidget.Skill;
        var targetTransform = skillWidget.DescriptionTransform;

        spawnedSkillDescriptionWidget = SkillDescriptionWidgetFactory.CreateSkillDescriptionWidget(skillDescriptionWidgetPrefab, targetTransform, skill);
    }
}