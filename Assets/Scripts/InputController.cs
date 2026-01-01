using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] Rigidbody rb;
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference mouseAction;
    Vector3 move;
    Vector2 mousePos;

    void Update()
    {
        mousePos = mouseAction.action.ReadValue<Vector2>();
        move = moveAction.action.ReadValue<Vector3>();
        mousePos = mouseAction.action.ReadValue<Vector2>();
        CameraRotation();
       
    }
    void OnEnable()
    {
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        mouseAction.action.Disable();
        mouseAction.action.performed -= OnMouse;
    }
    void FixedUpdate()
    {
        if (move != Vector3.zero)
        {
            MoveWASD();
        }

    }

    private void MoveWASD()
    {
        Vector3 direction =
            transform.forward * move.z +
            transform.right * move.x;

        Vector3 newPosition =
            rb.position + direction * speed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void CameraRotation()
    {
        float mouseX = mousePos.x;
        //transform.Rotate(0f, mouseX * 0.1f, 0f);
        Debug.Log(mousePos);
    }
    void OnMouse(InputAction.CallbackContext ctx)
    {
        //Debug.Log(ctx.ReadValue<Vector2>());
    }
}
