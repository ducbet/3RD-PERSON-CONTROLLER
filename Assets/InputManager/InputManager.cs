using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public PlayerControls playerControls;

    public Vector2 movementInput;
    public float verticalInput { get; private set; }
    public float horizontalInput { get; private set; }
    public float movementAmount { get; private set; }


    public Vector2 cameraInput;
    public float verticalCameraInput { get; private set; }
    public float horizontalCameraInput { get; private set; }

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
                //movementAmount = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));
                movementAmount = movementInput.magnitude;
            };
            playerControls.CameraMovement.Rotation.performed += context =>
            {
                cameraInput = context.ReadValue<Vector2>();
                verticalCameraInput = cameraInput.y;
                horizontalCameraInput = cameraInput.x;
            };
        }
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
