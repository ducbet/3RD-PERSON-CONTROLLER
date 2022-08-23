using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    Animator animator;

    int verticalAnimatorIndex;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        verticalAnimatorIndex = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorValue(float vertical)
    {
        animator.SetFloat(verticalAnimatorIndex, SnapFloatValue(vertical), 0.1f, Time.deltaTime);
    }

    private float SnapFloatValue(float val)
    {
        if (val > .55f)
        {
            return 1f;
        }
        else if (val > 0)
        {
            return .5f;
        }
        else if (val < -.55f)
        {
            return -1f;
        }
        else if (val < 0)
        {
            return -.5f;
        }
        return 0;
    }
}
