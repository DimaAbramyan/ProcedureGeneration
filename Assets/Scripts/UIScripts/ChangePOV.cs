using UnityEngine;

public class ChangePOV : MonoBehaviour
{
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject observerCamera;
    bool Step;
    public void CameraChange()
    {
        Step = !Step;
        playerCamera.SetActive(Step);
        observerCamera.SetActive(!Step);
    }
}
