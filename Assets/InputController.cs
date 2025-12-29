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
        move = moveAction.action.ReadValue<Vector3>();
        mousePos = mouseAction.action.ReadValue<Vector2>();
        Debug.Log(mousePos);

    }
    void OnEnable()
    {
        moveAction.action.Enable();
        mouseAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        mouseAction.action.Disable();
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
        Vector3 newPosition = rb.position + move * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
    //worldPos = mainCam.ScreenToWorldPoint(Mouse.current.position);
}
