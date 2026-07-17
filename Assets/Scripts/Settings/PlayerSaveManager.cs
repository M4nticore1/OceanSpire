using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    [SerializeField] private PlayerSettingsManager playerSettingsManager;
    [SerializeField] private TutorialManager tutorialManager;

    //public void SavePlayer()
    //{
    //    var playerData = PlayerData.Create(playerSettingsManager,
    //        tutorialManager);

    //    PlayerSaveSystem.SaveData(playerData);
    //}
}