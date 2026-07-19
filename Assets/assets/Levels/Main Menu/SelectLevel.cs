using UnityEngine;
using UnityEngine.UI;

public class SelectLevel : MonoBehaviour
{
    public GameObject Overlay;

    public void onCloseOverlayPress()
    {
        Overlay.SetActive(false);
    }
}
