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

    public float idleJumpInitVelocity = 10f;
    public float runningJumpRatio = 0.3f; // vs idleJumpInitVelocity
    public float runningJumpLeapVelocityX = 1;  // velocity.x *=  runningJumpLeapVelocityX

    public LayerMask groundLayer;

    private Vector3 direction;

    private bool isInteracting = false;
    private bool isJumping = false;
    private bool isGround = true;
    private bool isForwardJump = false;  // Do not play landing animatioin if jump forward

    public float fallingVelocity = 33f;
    private Vector3 leapingVelocity;
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
        HandleJumping();
        if (isInteracting)
        {
            // Do not handle movement, rotation while falling
            return;
        }
        if (isJumping)
        {
            // Do not handle movement, rotation while jumping
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
            HandleFallingForces();
        }
        if (isJumping)
        {
            // only handle forces while jumping (not reached the highest point)
            // handle falling, landing as normal (animations,...) after player reached the highest point
            return;
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
    private void HandleFallingForces()
    {
        inAirTime += Time.deltaTime;
        // Have to set velocity because we don't set velocity in HandleTranslation while falling
        playerRigidbody.velocity = SmoothFallingVelocityXZ();
        playerRigidbody.AddForce(Vector3.down * fallingVelocity * inAirTime);
    }

    private Vector3 SmoothFallingVelocityXZ()
    {
        float velocityY = playerRigidbody.velocity.y;
        Vector3 velocityXZ = Vector3.SmoothDamp(playerRigidbody.velocity, Vector3.zero, ref leapingVelocity, leapingVelocitySmoothTime);
        velocityXZ.y = velocityY;
        return velocityXZ;
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
        if (isForwardJump)
        {
            return;
        }
        if (isInteracting)
        {
            // Do not start falling animation again if still falling
            return;
        }
        animatorManager.PlayTargetAnimation(animatorManager.fallingAnimation, true);
    }

    private void HandleLanding()
    {
        if (isGround)
        {
            return;
        }
        Debug.Log("HandleLanding isGround: " + isGround + ", isInteracting " + isInteracting);
        if (isForwardJump)
        {
            inAirTime = 0;
            isGround = true;
            isForwardJump = false;
            animatorManager.PlayTargetAnimation(animatorManager.runningJumpLandingAnimation, true);
            return;
        }
        if (!isInteracting)
        {
            return;
        }
        // Only start landing if not on the ground and playing fallingAnimation animation
        animatorManager.PlayTargetAnimation(animatorManager.landingAnimation, true);
        isGround = true;
        inAirTime = 0;
    }

    private void HandleJumping()
    {
        /*
        1. isJumping == true and isGround == true -> prepare to jump (animation started but did not jump)
        2. isJumping == true and isGround == false -> leaved the ground but didn't reach the highest point
        3. isJumping == false and isGround == true -> idle on the ground
        4. isJumping == false and isGround == false -> start falling from the highest point
        */
        if (isJumping && isGround)
        {
            // Case 1
            //Debug.Log("Case 1 prepare to jump");
            return;
        }
        if (isJumping && !isGround)
        {
            // Case 2
            //Debug.Log("Case 2 leaved the ground but didn't reach the highest point");
            if (playerRigidbody.velocity.y < 0)
            {
                isJumping = false;
                // reached the highest point, start falling
                Debug.Log("HandleJumping reached the highest point, start falling, isJumping: " + isJumping + ", velocity: " + playerRigidbody.velocity);
            }
            //Debug.Log("isJumping: " + isJumping + ", velocity: " + playerRigidbody.velocity);
            return;
        }
        if (!isGround || !inputManager.isJumping)
        {
            // !isGround -> Case 4 (2 is above)
            //Debug.Log("Case 4. start falling from the highest point. isGround: " + isGround + ", inputManager.isJumping: " + inputManager.isJumping);
            return;
        }
        // case 3
        if (isInteracting)
        {
            // Do not jump if still in (landing) animation
            return;
        }
        // idle jump up or run jumping...
        if (GetMovementState() == MovementState.Idle)
        {
            //Debug.Log("Case 3 idle jump" + ", velocity :" + playerRigidbody.velocity + ", position: " + transform.position);
            HandleIdleJump();
        }
        else
        {
            //Debug.Log("Case 3 running jump" + ", velocity :" + playerRigidbody.velocity + ", position: " + transform.position);
            HandleRunningJump();
        }
        isJumping = true;
    }
    private void HandleRunningJump()
    {
        animatorManager.PlayTargetAnimation(animatorManager.runningJumpAnimation, true);
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x * runningJumpLeapVelocityX, idleJumpInitVelocity * runningJumpRatio, playerRigidbody.velocity.z);
        isGround = false;
        isForwardJump = true;
        inAirTime = 0;
        Debug.Log("HandleRunningJump isJumping: " + isJumping + ", isGround: " + isGround + ", start velocity: " + playerRigidbody.velocity);
    }

    private void HandleIdleJump()
    {
        animatorManager.PlayTargetAnimation(animatorManager.idleJumpUpAnimation, false);
        StartCoroutine(StartJumping());
    }
    private IEnumerator StartJumping()
    {
        // jumping animation doesn't jump up immediately.
        // Need to delay real jumping (change position Y) for a while
        yield return new WaitForSeconds(animatorManager.jumpUpDelayTime);
        Vector3 currentVelocity = playerRigidbody.velocity;
        currentVelocity.y = idleJumpInitVelocity;
        playerRigidbody.velocity = currentVelocity;
        Debug.Log("isJumping: " + isJumping + ", start velocity: " + playerRigidbody.velocity);
        isGround = false;
        inAirTime = 0;
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
        return MovementState.Idle;
    }
}
