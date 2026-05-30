using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    [SerializeField] Transform playerBody;          // объект игрока
    [SerializeField] InputActionReference mouseAction;

    [SerializeField] float sensitivity = 3f;
    [SerializeField] float minY = -80f;
    [SerializeField] float maxY = 80f;

    float xRotation = 0f;

    void OnEnable()
    {
        mouseAction.action.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        mouseAction.action.Disable();
    }

    void Update()
    {
        Vector2 mouseDelta = mouseAction.action.ReadValue<Vector2>();

        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
