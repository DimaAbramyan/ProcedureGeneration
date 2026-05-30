using UnityEngine;

using UnityEngine;

public class ObserverCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 5000f;

    private bool dragging;
    private Vector3 lastMousePos;

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A D
        float v = Input.GetAxisRaw("Vertical");   // W S

        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;

        transform.position += move;
    }

    void HandleZoom()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragging = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            dragging = false;
        }

        if (!dragging)
            return;

        float deltaY = Input.mousePosition.y - lastMousePos.y;
        lastMousePos = Input.mousePosition;

        Vector3 pos = transform.position;

        pos.y -= deltaY * zoomSpeed * Time.deltaTime;

        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);

        transform.position = pos;
    }
}