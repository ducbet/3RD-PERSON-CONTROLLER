using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public PlayerControls playerControls;

    public Vector2 movementInput;
    public float verticalInput { get; private set; }
    public float horizontalInput { get; private set; }


    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed += context =>
            {
                movementInput = context.ReadValue<Vector2>();
                verticalInput = movementInput.y;
                horizontalInput = movementInput.x;
            };
        }
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
