using UnityEngine;

public class NameData
{
    public int firstNameId { get; private set; }
    public int lastNameId { get; private set; }

    public NameData(int firstNameId, int lastNameIf)
    {
        this.firstNameId = firstNameId;
        this.lastNameId = lastNameIf;
    }
}