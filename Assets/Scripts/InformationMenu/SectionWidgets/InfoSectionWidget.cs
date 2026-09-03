using UnityEngine;

public abstract class InfoSectionWidget : MonoBehaviour
{
    public void Init(InfoSectionData sectionData)
    {
        HandleInit(sectionData);
    }

    protected virtual void HandleInit(InfoSectionData sectionData)
    {

    }
}