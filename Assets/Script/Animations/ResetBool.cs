using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBool : StateMachineBehaviour
{
    public string[] defaultTrueBoolNames;
    public string[] defaultFalseBoolNames;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var boolName in defaultTrueBoolNames)
        {
            animator.SetBool(boolName, true);
        }

        foreach (var boolName in defaultFalseBoolNames)
        {
            animator.SetBool(boolName, false);
        }
    }

}
