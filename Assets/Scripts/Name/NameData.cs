using UnityEngine;

public class NameData
{
    public int FirstNameId = 0;
    public int LastNameId = 0;

    public static NameData Create(NameComponent nameComponent)
    {
        return new NameData()
        {
            FirstNameId = nameComponent.FirstNameId,
            LastNameId = nameComponent.LastNameId,
        };
    }
}