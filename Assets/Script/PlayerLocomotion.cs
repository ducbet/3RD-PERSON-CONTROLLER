using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    public enum MovementState { Idle, Walking, Running, Sprinting }

    InputManager inputManager;
    Transform cameraTransform;
    Rigidbody playerRigidbody;
    AnimatorManager animatorManager;

    public float walkingSpeed = 100f;
    public float runningSpeed = 300f;
    public float sprintingSpeed = 350f;
    public float rotateSpeed = 5f;
    public float runningThreshold = .55f;

    public LayerMask groundLayer;

    private Vector3 direction;

    private bool isInteracting = false;
    private bool isGround = true;

    public float fallingVelocity = 33f;
    public float leapingVelocity;
    public float leapingVelocitySmoothTime = 2f;
    private float inAirTime = 0;
    public float startLandingHeight = 1f;
    private Vector3 groundCheckOriginOffset = new Vector3(0f, 0.5f, 0f);

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        cameraTransform = Camera.main.transform;
        playerRigidbody = GetComponent<Rigidbody>();
        animatorManager = GetComponent<AnimatorManager>();

        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }

    private void Update()
    {
        isInteracting = animatorManager.GetBool(animatorManager.isInteractingName);
    }

    public void HandleMovementAnimation()
    {
        animatorManager.UpdateAnimatorValue(GetMovementState());
    }

    public void HandleMovement()
    {
        HandleFallingAndLanding();

        if (isInteracting)
        {
            // Do not handle movement, rotation while falling
            return;
        }
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

    private void HandleTranslation()
    {
        float speed = GetMovementSpeed(GetMovementState());
        playerRigidbody.velocity = direction * speed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        if (direction == Vector3.zero) return;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        transform.rotation = lookRotation;
    }

    private void HandleFallingAndLanding()
    {
        if (!isGround)
        {
            inAirTime += Time.deltaTime;
            // Have to set velocity because we don't set velocity in HandleTranslation while falling
            float leapingVelocityZ = Mathf.SmoothDamp(playerRigidbody.velocity.z, 0, ref leapingVelocity, leapingVelocitySmoothTime);
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, playerRigidbody.velocity.y, leapingVelocityZ);
            playerRigidbody.AddForce(Vector3.down * fallingVelocity * inAirTime);
        }
        if (IsFalling())
        {
            HandleFalling();
        }
        else
        {
            HandleLanding();
        }
    }

    private bool IsFalling()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + groundCheckOriginOffset, 0.2f, Vector3.down, out hit, startLandingHeight, groundLayer))
        {
            return false;
        }
        return true;
    }

    private void HandleFalling()
    {
        isGround = false;
        if (isInteracting)
        {
            // Do not start fallingAnimation again if still falling
            return;
        }
        animatorManager.PlayTargetAnimation(animatorManager.fallingAnimation, true);
    }

    private void HandleLanding()
    {
        if (!isGround && isInteracting)
        {
            // Only start landing if not on the ground and playing fallingAnimation animation
            animatorManager.PlayTargetAnimation(animatorManager.landingAnimation, true);
            inAirTime = 0;
            isGround = true;
        }
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

    private MovementState GetMovementState()
    {
        float movementAmout = inputManager.movementAmount;
        bool isSprinting = inputManager.isSprinting;
        bool isWalking = inputManager.isWalking;
        if (movementAmout > runningThreshold)
        {
            if (isSprinting)
            {
                return MovementState.Sprinting;
            }
            if (isWalking)
            {
                return MovementState.Walking;
            }
            return MovementState.Running;
        }
        else if (movementAmout > 0)
        {
            return MovementState.Walking;
        }
        return 0;
    }
}
