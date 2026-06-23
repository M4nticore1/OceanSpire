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
            Debug.LogError("skillWidget is not valid");
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
        if (!spawnedSkillDescriptionWidget) return;
        if (spawnedSkillDescriptionWidget.Skill != skillWidget.Skill) return;

        Destroy(spawnedSkillDescriptionWidget.gameObject);
        spawnedSkillDescriptionWidget = null;
    }

    private void SpawnDescriptionWidget(SkillWidget skillWidget)
    {
        var skill = skillWidget.Skill;
        var targetTransform = skillWidget.DescriptionTransform;

        spawnedSkillDescriptionWidget = SkillDescriptionWidgetFactory.CreateSkillDescriptionWidget(skillDescriptionWidgetPrefab, targetTransform, skill);
    }
}