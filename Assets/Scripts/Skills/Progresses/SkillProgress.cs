using UnityEngine;

public abstract class SkillProgress : MonoBehaviour
{
    [SerializeField] private SkillAdapter skillAdapter;
    public SkillAdapter SkillAdapter => skillAdapter;

    [SerializeField] private float xpGain;
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

    protected void AddXp(float xp)
    {
        foreach (var skill in skillAdapter.GetSkills()) {
            skill.AddXp(xp);
        }
    }
}