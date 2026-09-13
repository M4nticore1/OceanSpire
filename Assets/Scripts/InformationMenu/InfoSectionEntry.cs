using UnityEngine;

public class InfoSectionEntry
{
    public InfoPanelWidget SectionWidget { get; private set; }
    public InfoPanelData SectionData { get; private set; }

    public InfoSectionEntry(InfoPanelWidget widget, InfoPanelData data)
    {
        SectionWidget = widget;
        SectionData = data;
    }
}