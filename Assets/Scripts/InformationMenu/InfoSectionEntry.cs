using UnityEngine;

public class InfoSectionEntry
{
    public InfoSectionWidget SectionWidget { get; private set; }
    public InfoSectionData SectionData { get; private set; }

    public InfoSectionEntry(InfoSectionWidget widget, InfoSectionData data)
    {
        SectionWidget = widget;
        SectionData = data;
    }
}