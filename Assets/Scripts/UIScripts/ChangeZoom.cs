using UnityEngine;

public class ChangeZoom : MonoBehaviour
{
    [SerializeField]
    GameObject camera;
    [SerializeField]
    Vector3[] cameraPositions;
    public void OnButtonClick(int value)
    {
        camera.transform.position = cameraPositions[value];
    }
}
