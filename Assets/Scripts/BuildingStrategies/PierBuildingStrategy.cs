using UnityEngine;

public class PierBuildingStrategy : BuildingStrategy
{
    public override void OnEnter(EntityCityNavigator navigator)
    {

    }

    public override void OnExit(EntityCityNavigator navigator)
    {

    }

    public override void OnSetInteractBuilding(EntityInteractor navigator)
    {
        Debug.Log("OnSetInteractBuilding");
        Human human = navigator.GetComponent<Human>();
        if (human) {
            EntityInteractor interactor = human.interactor;
            int interactorIndex = interactor.interactorIndex;
            Boat boat = CityManager.Instance.citizenBoats[interactorIndex];
            human.boatRider.SetBoat(boat);
        }
    }

    public override void OnRemoveInteractBuilding(EntityInteractor navigator)
    {
        Debug.Log("OnRemoveInteractBuilding");
        Human human = navigator.GetComponent<Human>();
        if (human) {
            EntityInteractor interactor = human.interactor;
            human.boatRider.RemoveBoat();
        }
    }

    public override void OnStartInteracting(EntityInteractor interactor)
    {
        Debug.Log("OnStartInteracting");
        Human human = interactor.GetComponent<Human>();
        if (human) {
            human.boatRider.StartEnteringBoat();
        }
    }

    public override void OnStopInteracting(EntityInteractor interactor)
    {
        
    }

    public override void OnInteracting(EntityInteractor interactor)
    {
        
    }
}
