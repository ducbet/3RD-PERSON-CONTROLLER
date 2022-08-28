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
    public string fallingAnimation = "Falling";
    [HideInInspector]
    public string landingAnimation = "Landing";

    private float fadeLength = 0.2f;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void PlayTargetAnimation(string animationName, bool isInteracting)
    {
        animator.SetBool(HashString(isInteractingName), isInteracting);
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
}
