using UnityEngine;

public class ResidentLevelUpDailyTaskController : DailyTaskController
{
    private CreaturesLoader creaturesLoader => CreaturesLoader.Instance;

    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;

        Human.OnHumanSkillLevelUp += HandleHumanSkillLevelUp;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;

        Human.OnHumanSkillLevelUp -= HandleHumanSkillLevelUp;

        return true;
    }

    private void HandleHumanSkillLevelUp(Human human)
    {
        if (human == null) return;

        if (!creaturesLoader.IsLoaded) return;

        var resident = human as Citizen;
        if (resident == null) return;

        AddTaskProgress(1);
    }
}