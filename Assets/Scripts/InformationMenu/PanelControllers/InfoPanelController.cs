using UnityEngine;

public abstract class InfoPanelController : MonoBehaviour
{
    [SerializeField] private InfoPanelWidget infoPanel;

    public void SetInformationable(IInformationable informationable)
    {
        if (informationable == null) {
            Debug.LogError($"[{nameof(InfoPanelController)}] Informationable is not valid at {gameObject}!");
            return;
        }

        if (infoPanel == null) {
            Debug.LogError($"[{nameof(InfoPanelController)}] Info Panel is not valid at {gameObject}!");
            return;
        }

        if (ShouldDisplay(informationable)) {
            infoPanel.Show(GetData(informationable));
        }
        else {
            infoPanel.Hide();
        }
    }

    protected abstract bool ShouldDisplay(IInformationable informationable);
    protected abstract InfoPanelData GetData(IInformationable informationable);
}