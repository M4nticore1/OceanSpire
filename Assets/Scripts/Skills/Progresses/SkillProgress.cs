using UnityEngine;

public abstract class SkillProgress : MonoBehaviour
{
    [SerializeField] private SkillAdapter skillAdapter;

    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    protected virtual bool TrySubscribe()
    {
        if (isSubscribed) return false;

        return true;
    }

    protected virtual bool TryUnsubscribe()
    {
        if (!isSubscribed) return false;

        return true;
    }

    protected void AddXp()
    {
        foreach (var skill in skillAdapter.GetSkills()) {
            var xp = skill.SkillDefinition.XpGainRate;
            skill.AddXp(xp);
        }
    }
}