using UnityEngine;

public class SpoonKilerAnimation : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Archer>()?.ThrowArrow();
    }
}
