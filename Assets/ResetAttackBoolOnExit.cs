using UnityEngine;

public class ResetAttackBoolOnExit : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // This sets our bool back to false when the attack is done.
        animator.SetBool("isAttacking", false);
    }
}