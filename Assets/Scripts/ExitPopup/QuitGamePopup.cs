using UnityEngine;

public class QuitGamePopup : ExitPopup
{
    protected override void HandleExitButtonClicked()
    {
        Application.Quit();
    }
}