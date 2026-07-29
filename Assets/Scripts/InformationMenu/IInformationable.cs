using UnityEngine;

public interface IInformationable
{
    public LocalizationItem GetInformationName();
    public LocalizationItem GetInformationDescription();
    public Sprite GetInformationImage();
}