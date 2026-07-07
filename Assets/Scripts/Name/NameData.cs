using System;
using UnityEngine;

[Serializable]
public class NameData
{
    public int FirstNameId = 0;
    public int LastNameId = 0;

    public static NameData Default()
    {
        return new NameData();
    }

    public static NameData Random(GenderComponent genderComponent, HumanNamesList namesList)
    {
        return new NameData()
        {
            FirstNameId = genderComponent.IsMale ? namesList.GetRandomMaleFirstNameId() : namesList.GetRandomFemaleFirstNameId(),
            LastNameId = genderComponent.IsMale ? namesList.GetRandomMaleLastNameId() : namesList.GetRandomFemaleLastNameId()
        };
    }

    public static NameData Create(NameComponent nameComponent)
    {
        return new NameData()
        {
            FirstNameId = nameComponent.FirstNameId,
            LastNameId = nameComponent.LastNameId,
        };
    }
}