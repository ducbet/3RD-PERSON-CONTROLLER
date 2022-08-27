using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    public enum MovementState { Idle, Walking, Running, Sprinting} 

    InputManager inputManager;
    Transform cameraTransform;
    Rigidbody playerRigidbody;
    AnimatorManager animatorManager;

    public float walkingSpeed = 100f;
    public float runningSpeed = 300f;
    public float sprintingSpeed = 350f;
    public float rotateSpeed = 5f;
    public float runningThreshold = .55f;
    private Vector3 direction;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        cameraTransform = Camera.main.transform;
        playerRigidbody = GetComponent<Rigidbody>();
        animatorManager = GetComponent<AnimatorManager>();
    }

    public void HandleMovementAnimation()
    {
        animatorManager.UpdateAnimatorValue(GetMovementState(inputManager.movementAmount, inputManager.isSprinting));
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
        float speed = GetMovementSpeed(GetMovementState(inputManager.movementAmount, inputManager.isSprinting));
        playerRigidbody.velocity = direction * speed * Time.deltaTime;
    }

    public void HandleRotation()
    {
        if (direction == Vector3.zero) return;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        transform.rotation = lookRotation;
    }

    private float GetMovementSpeed(MovementState state)
    {
        if (state == MovementState.Sprinting)
        {
            return sprintingSpeed;
        }
        if (state == MovementState.Running)
        {
            return runningSpeed;
        }
        if (state == MovementState.Walking)
        {
            return walkingSpeed;
        }
        return 0;
    }

    private MovementState GetMovementState(float val, bool isSprinting)
    {
        if (val > runningThreshold)
        {
            if (isSprinting)
            {
                return MovementState.Sprinting;
            }
            return MovementState.Running;
        }
        else if (val > 0)
        {
            return MovementState.Walking;
        }
        return 0;
    }
}
