using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    Animator animator;

    private int verticalAnimatorIndex;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        verticalAnimatorIndex = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorValue(PlayerLocomotion.MovementState movementStatus)
    {
        animator.SetFloat(verticalAnimatorIndex, (float) movementStatus, 0.1f, Time.deltaTime);
    }

}
