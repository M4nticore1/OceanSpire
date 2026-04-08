using UnityEngine;

public class HumanContextMenu : ContextMenu<Human>
{
    public override void Init(Human entity)
    {
        //SetNameText(human.firstName + " " + human.lastName);
    }
}