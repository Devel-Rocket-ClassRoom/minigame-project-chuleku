using UnityEngine;

public class ArcherAnimatorAttack : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Archer>()?.ThrowArrow();
    }
}
