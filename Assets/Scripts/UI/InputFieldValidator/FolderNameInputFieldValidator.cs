using UnityEngine;

public class FolderNameInputFieldValidator : InputFieldValidator
{
    protected override bool IsValid(string text)
    {
        return DirectoryUtils.IsFolderNameValid(text);
    }
}