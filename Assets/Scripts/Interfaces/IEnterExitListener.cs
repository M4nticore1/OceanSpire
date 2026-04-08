using UnityEngine;

public interface IEnterExitListener
{
    public void OnEnterBuilding(CreatureCityNavigator navigator);
    public void OnExitBuilding(CreatureCityNavigator navigator);
}
