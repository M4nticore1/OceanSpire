using UnityEngine;

public class DailyTaskInstance
{
    public DailyTaskDefinition definition { get; private set; }
    public int progress { get; private set; } = 0;

    public DailyTaskInstance(DailyTaskDefinition definition, int progress)
    {
        this.definition = definition;
    }
}