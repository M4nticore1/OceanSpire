using UnityEngine;

public interface IEnterExitListener
{
    public void OnEnterBuilding(EntityCityNavigator navigator);
    public void OnExitBuilding(EntityCityNavigator navigator);
}
