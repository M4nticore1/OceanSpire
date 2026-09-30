using UnityEngine;

public class WorldNameLengthInputFieldValidator : InputFieldValidator
{
    protected override bool IsValid(string text)
    {
        if (text.Length <= 0) return false;
        if (text.Length > 32) return false;

        return true;
    }
}