using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillsPanel : MonoBehaviour
{
    [SerializeField] private SkillWidget skillWidgetPrefab;

    [SerializeField] private LayoutGroup layoutGroup;
    private List<SkillWidget> spawnedSkillWidgets = new();

    public void SetSkills(SkillsComponent skillsComponent)
    {
        if (!skillsComponent) {
            Debug.LogError("skillsComponent is not valid", this);
            return;
        }

        RemoveWidgets();
        CreateWidgets(skillsComponent);
    }

    private void CreateWidgets(SkillsComponent skills)
    {
        foreach (var skill in skills.Skills.Values) {
            var widget = SkillWidgetFactory.CreateSkillWidget(skillWidgetPrefab, layoutGroup.transform, skill);
            spawnedSkillWidgets.Add(widget);
        }
    }

    private void RemoveWidgets()
    {
        for (int i = spawnedSkillWidgets.Count - 1; i >= 0; i--) {
            Destroy(spawnedSkillWidgets[i].gameObject);
            spawnedSkillWidgets.RemoveAt(i);
        }
    }
}