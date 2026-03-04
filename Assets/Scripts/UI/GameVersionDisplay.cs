using TMPro;
using UnityEngine;

public class GameVersionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameVersionText;

    private void Start()
    {
        string version = Application.version;
        gameVersionText.SetText(version);
    }
}
