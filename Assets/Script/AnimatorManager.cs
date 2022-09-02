using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    private Animator animator;
    private Dictionary<string, int> animationHash = new Dictionary<string, int>();

    [HideInInspector]
    public string moveVerticalName = "Vertical";
    [HideInInspector]
    public string isInteractingName = "IsInteracting";
    [HideInInspector]
    public string useRootMotionName = "UseRootMotion";
    
    [HideInInspector]
    public string fallingAnimation = "Falling";
    [HideInInspector]
    public string landingAnimation = "Landing";
    [HideInInspector]
    public string idleJumpUpAnimation = "Idle Jumping Up";
    public float jumpUpDelayTime = 0.33f;

    [HideInInspector]
    public string runningJumpAnimation = "Running Jump";
    [HideInInspector]
    public string runningJumpLandingAnimation = "Running Jump Landing";

    [HideInInspector]
    public string dodgingBackAnimation = "Dodging Back";

    [HideInInspector]
    public Vector3 deltaPosition = Vector3.zero;

    private float fadeLength = 0.2f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void PlayTargetAnimation(string animationName, bool isInteracting, bool useRootMotion = false)
    {
        deltaPosition = Vector3.zero;

        animator.SetBool(HashString(isInteractingName), isInteracting);
        animator.SetBool(HashString(useRootMotionName), useRootMotion);
        animator.CrossFade(HashString(animationName), fadeLength);
    }

    public void UpdateAnimatorValue(PlayerLocomotion.MovementState movementState)
    {
        animator.SetFloat(HashString(moveVerticalName), (float) movementState, 0.1f, Time.deltaTime);
    }

    private int HashString(string animationName)
    {
        if (animationHash.ContainsKey(animationName)) {
            return animationHash[animationName];
        }
        animationHash[animationName] = Animator.StringToHash(animationName);
        return animationHash[animationName];
    }

    public bool GetBool(string boolName)
    {
        return animator.GetBool(HashString(boolName));
    }

    private void OnAnimatorMove()
    {
        if (GetBool(useRootMotionName))
        {
            deltaPosition = animator.deltaPosition / Time.deltaTime;
        }
    }
}
