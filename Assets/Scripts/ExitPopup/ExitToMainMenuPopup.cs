using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToMainMenuPopup : ExitPopup
{
    protected override void HandleExitButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}