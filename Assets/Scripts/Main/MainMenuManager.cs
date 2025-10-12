using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button loadSaveButton = null;
    [SerializeField] private Button deleteSaveButton = null;

    private int selectedSaveIndex = 0;

    private void Awake()
    {
        loadSaveButton.onClick.AddListener(LoadSave);
        deleteSaveButton.onClick.AddListener(DeleteSave);
    }

    private void LoadSave()
    {
        SceneManager.LoadScene(1);
    }

    private void DeleteSave()
    {
        SaveSystem.DeleteSave(0);
    }
}
