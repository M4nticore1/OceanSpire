using UnityEngine;

public abstract class SkillProgress : MonoBehaviour
{
    [SerializeField] private SkillAdapter skillAdapter;
    public SkillAdapter SkillAdapter => skillAdapter;

    [SerializeField] private float xpGain = 0.1f;
    public float XpGain => xpGain;

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
            skill.AddXp(xpGain);
        }
    }

    protected void AddXp(float xp)
    {
        foreach (var skill in skillAdapter.GetSkills()) {
            skill.AddXp(xp);
        }
    }

    protected void AddXp(SkillsComponent skillsComponent)
    {
        AddXp(skillsComponent, xpGain);
    }

    protected void AddXp(SkillsComponent skillsComponent, float xp)
    {
        if (skillsComponent == null) return;
        if (skillAdapter == null) return;

        var skill = skillsComponent.GetSkill(skillAdapter.SkillId);
        if (skill == null) return;

        skill.AddXp(xp);
    }
}