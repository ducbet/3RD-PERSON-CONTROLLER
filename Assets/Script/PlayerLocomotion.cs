using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    InputManager inputManager;
    Transform cameraTransform;
    Rigidbody playerRigidbody;

    public float movementSpeed = 350f;
    public float rotateSpeed = 15f;
    private Vector3 direction;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        cameraTransform = Camera.main.transform;
        playerRigidbody = GetComponent<Rigidbody>();
    }

    public void HandleMovement()
    {
        CalculateDirection();
        HandleTranslation();
        HandleRotation();
    }

    private void CalculateDirection()
    {
        direction = cameraTransform.forward * inputManager.verticalInput + cameraTransform.right * inputManager.horizontalInput;
        direction.y = 0;
        direction.Normalize();
    }

    public void HandleTranslation()
    {
        playerRigidbody.velocity = direction * movementSpeed * Time.deltaTime;
    }

    public void HandleRotation()
    {
        if (direction == Vector3.zero) return;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        transform.rotation = lookRotation;
    }
}
