using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillsList", menuName = "Game Content/SkillsList")]
public class SkillsList : ScriptableObject
{
    [SerializeField] private SkillDefinition[] skillDefinitions;
    public Dictionary<SkillId, SkillDefinition> SkillDefinitionsDict = new();

    private static SkillsList _instance;
    public static SkillsList Instance
    {
        get
        {
            if (!_instance) {
                _instance = Resources.Load<SkillsList>("Lists/SkillsList");
                _instance.Init();
            }

            return _instance;
        }
    }

    private void Init()
    {
        foreach (var def in skillDefinitions) {
            if (SkillDefinitionsDict.ContainsKey(def.SkillId)) {
                Debug.LogError($"Duplicate SkillId: {def.SkillId}");
                continue;
            }

            SkillDefinitionsDict[def.SkillId] = def;
        }
    }

    public SkillDefinition GetSkillDefinition(SkillId id)
    {
        return SkillDefinitionsDict[id];
    }
}